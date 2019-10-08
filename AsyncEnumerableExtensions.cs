using System.IO;

static class AsyncEnumerableExtensions
{
    public static void Explain(this AsyncEnumerable runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- `IAsyncEnumerable<T>` allows to write asynchronous pull based streams, similar to regular enumerables with `yield return` and `yield break`
- `WithCancellation` only adds the token to the enumerator but doesn't influence the state machine
- `WithCancellation` in combination with `[EnumeratorCancellation]` can be used to create a combined token
");
    }
}