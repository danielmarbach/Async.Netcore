using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

static class PowerfulAsyncEnumerableExtensions
{
    public static void Explain(this PowerfulAsyncEnumerable runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Async Enumerable can be combined in powerful ways with other async and TPL constructs such as `Task.WhenAny`
");
    }

    public static void PrintStatistics(
        this PowerfulAsyncEnumerable runnable,
        List<PackageMetadata> packagesWithNetFxAsms,
        int totalCount)
    {
        Console.Out.WriteLine();
        Console.Out.WriteLine($"Packages with dotnet assemblies: {packagesWithNetFxAsms.Count} ({totalCount})");
        Console.Out.WriteLine($"Total download size(MB): {packagesWithNetFxAsms.Sum(p => p.Size) / 1000000.0}");
    }

    public static async Task<CatalogPage> ReadPage5000FromNuget(
        this PowerfulAsyncEnumerable runnable,
        HttpClient httpClient)
    {
        var url = $"https://api.nuget.org/v3/catalog0/page5000.json";
        var page = await runnable.ReadUrl(httpClient, url).ConfigureAwait(false);
        Console.WriteLine($"Reading metadata for {url}");
        return page;
    }

    public static async Task<CatalogPage> ReadUrl(
        this PowerfulAsyncEnumerable runnable,
        HttpClient httpClient,
        string catalogPageUrl)
    {
        var response = await httpClient.GetAsync(catalogPageUrl).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<CatalogPage>(responseStream).ConfigureAwait(false);
    }



    public static async Task<PackageMetadata> GetMetadataForPackage(
        this PowerfulAsyncEnumerable runnable,
        HttpClient httpClient,
        string packageMetadataUrl,
        SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        try
        {
            await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);

            return await PackageMetadataReader.ReadUrl(httpClient, packageMetadataUrl, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            throttler.Release();
        }
    }

    public static class PackageMetadataReader
    {

        public static async Task<PackageMetadata> ReadUrl(
            HttpClient httpClient,
            string packageMetadataUrl,
            CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(packageMetadataUrl, cancellationToken);

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<PackageMetadata>(responseStream, cancellationToken: cancellationToken);
        }
    }

    public class CatalogPage
    {
        [JsonPropertyName("commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

        [JsonPropertyName("items")]
        public List<CatalogItem> Items { get; set; }

    }

    public class CatalogItem
    {
        [JsonPropertyName("@id")]
        public string Url { get; set; }

        [JsonPropertyName("nuget:id")]
        public string PackageId { get; set; }

        [JsonPropertyName("nuget:version")]
        public string Version { get; set; }

        [JsonPropertyName("@type")]
        public string Type { get; set; }
    }

    public class PackageMetadata
    {
        public PackageMetadata()
        {
            PackageEntries = new List<PackageEntry>();
        }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("packageSize")]
        public long Size { get; set; }

        [JsonPropertyName("packageEntries")]
        public List<PackageEntry> PackageEntries { get; set; }

        public bool HasNetAssemblies
        {
            get
            {
                return PackageEntries.Any(pe => pe.FullName.ToLower().StartsWith("lib/"));
            }
        }

        public class PackageEntry
        {
            [JsonPropertyName("fullName")]
            public string FullName { get; set; }
        }
    }
}
