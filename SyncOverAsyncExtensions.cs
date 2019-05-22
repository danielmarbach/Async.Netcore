
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

static class SyncOverAsyncExtensions
{

    static LimitedConcurrencyLevelTaskScheduler scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
    public static void Explain(this SyncOverAsync runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- `Task.Result` or `Task.Wait` on asynchronous operations is much worse than calling truly synchronous APIs. Here is what happens
  - An asynchronous operation is kicked off. 
  - The calling thread is blocked waiting for that operation to complete.
  When the asynchronous operation completes, it unblocks the code waiting on that operation. This takes place on another thread.
- This leads to thread-pool starvation and service outages due to 2 threads being used instead of 1 to complete synchronous operations.
- If a synchronization context is available it can even lead to deadlocks
");
    }

    public static Task WrapInContext(this SyncOverAsync runnable, Action action)
    {
        return Task.Factory.StartNew(() =>
        {
            action();
        }, CancellationToken.None, TaskCreationOptions.None, scheduler);
    }

    public static async Task<string> DoAsyncOperation(this SyncOverAsync runnable)
    {
        await Task.Delay(2);
        return "Hello";
    }
}

