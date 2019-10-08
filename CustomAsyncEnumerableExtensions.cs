using System.IO;

static class CustomAsyncEnumerableExtensions
{
    public static void Explain(this CustomAsyncEnumerable runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- `WithCancellation` instructs the compiler generated statemachine to call `GetAsyncEnumerator` with the provided token. Otherwise `default(CancellationToken)` will be used.
");
    }
}