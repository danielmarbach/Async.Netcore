using System;
using System.IO;
using System.Threading;

static class GoodCitizenExtensions
{
    public static void Explain(this GoodCitizen runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Is async all the way and embraces virality of the API where it makes sense (IO-bound path)
- Async void methods will crash the process if an exception is thrown
- `Task`-returning methods are better since unhandled exceptions trigger the `TaskScheduler.UnobservedTaskException`
- Be ware that only when the finalizers are run the unobserved exception is thrown
- Does not explicitely offload to the worker thread pool by using `Task.Run` or `Task.Factory.StartNew` (offloading is a concern of the caller)
- Returns `Task`, `Task<TResult>`, `ValueTask` or `ValueTask<TResult>`
- Accepts `CancellationToken` if cancellation is appropriate. It is OK to add it but not respect it. Cancellation is cooperative.
- Tries to respect token where it can and makes sense
- Disposes owned `CancellationTokenSource` (due to allocated Timer instances when used with `CancelAfter`)
- Disposes owned `CancelationTokenRegistration` to not leak memory
- For high-perf scenarios avoid closure capturing in token registrations
- For library or framework code uses `ConfigureAwait(false)` to opt-out from context capturing if not needed
- Can used linked token sources for internal SLAs and cancellation scenarios
- Favours simple sequential async execution over explicit concurrency (concurrency is hard)
- No silver bullet that magically makes your DB query fasters ;)
");
    }

    public static void PrintStart(this GoodCitizen runnable)
    {
        Console.WriteLine($"Starting on {Thread.CurrentThread.ManagedThreadId}");
    }

    public static void PrintEnd(this GoodCitizen runnable)
    {
        Console.WriteLine($"End on {Thread.CurrentThread.ManagedThreadId}");
    }
}
