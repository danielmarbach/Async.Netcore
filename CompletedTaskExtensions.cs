using System.IO;

static class CompletedTaskExtensions {
    public static void Explain(this CompletedTask runnable, TextWriter writer) {
              writer.WriteLine(@"
- For pre-computed results, there's no need to call `Task.Run`, that will end up queuing a work item to the thread pool that will immediately complete with the pre-computed value. Instead, use `Task.FromResult`, to create a task wrapping already computed data.
- Alternatively `ValueTask<TResult>` can be used if low allocations are desired.
");  
    }
}