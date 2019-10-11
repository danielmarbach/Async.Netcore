using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static PowerfulAsyncEnumerableExtensions;

class PowerfulAsyncEnumerable : IRunnable
{
    public async Task Run()
    {
        var httpClient = new HttpClient();

        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var packagesWithNetFxAsms = new List<PackageMetadata>();
        var totalCount = 0;

        var catalogPage = await this.ReadPage5000FromNuget(httpClient).ConfigureAwait(false);

        await foreach (var package in this.ReadPackageMetadata(httpClient, catalogPage, tokenSource.Token)
            .ConfigureAwait(false))
        {
            totalCount++;
            if (package.HasNetAssemblies)
            {
                packagesWithNetFxAsms.Add(package);
                Console.Write("!");
            }
            else
            {
                Console.Write("~");
            }
        }

        this.PrintStatistics(packagesWithNetFxAsms, totalCount);
    }

    public async IAsyncEnumerable<PackageMetadata> ReadPackageMetadata(
        HttpClient httpClient,
        CatalogPage catalogPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        int maxConcurrentHttpCalls = 10)
    {
        using var throttler = new SemaphoreSlim(maxConcurrentHttpCalls);

        var tasks = new List<Task<PackageMetadata>>();
        foreach (var item in catalogPage.Items.Where(p => p.Type == "nuget:PackageDetails"))
        {
            tasks.Add(this.GetMetadataForPackage(httpClient, item.Url, throttler, cancellationToken));
        }

        while (tasks.Count > 0)
        {
            var done = await Task.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(done);

            yield return await done
                .ConfigureAwait(false);
        }
    }
}
