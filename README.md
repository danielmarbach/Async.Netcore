## AsyncDisposable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

## AsyncEnumerable

- `IAsyncEnumerable<T>` allows to write asynchronous pull based streams, similar to regular enumerables with `yield return` and `yield break`
- `WithCancellation` only adds the token to the enumerator but doesn't influence the state machine
- `WithCancellation` in combination with `[EnumeratorCancellation]` can be used to create a combined token

## Channels

- Low-level async primitives that allows to build higher level primitives like Dataflow library for example.
- Influenced by Go channels
- Data structure for publish/subscribe scenarios
- Allows decoupling of publishes from subscribers
- Can be combined in-memory or combined with pipelines to buffer network input and output
- Very good overview can be found in [Nicolas Portmann's article](https://ndportmann.com/system-threading-channels/)

## CompletedTask

- For pre-computed results, there's no need to call `Task.Run`, that will end up queuing a work item to the thread pool that will immediately complete with the pre-computed value. Instead, use `Task.FromResult`, to create a task wrapping already computed data.
- Alternatively `ValueTask<TResult>` can be used if low allocations are desired.

## CustomAsyncEnumerable

- `WithCancellation` instructs the compiler generated statemachine to call `GetAsyncEnumerator` with the provided token. Otherwise `default(CancellationToken)` will be used.

## CustomValueTaskSource

- Very powerful tool to write cached task sources without dropping deep to the async machinery
- Mostly for framework and library authors

## DefaultInterfaces

- Finally we can evolve interfaces as well.
- They can use the regular keywords.
- The compiler generates (of course with the async statemachine that I omitted here)
```
public interface IRun
{
  Task RunAsync();

  // added in v2
  async Task Run()
  {
    await this.RunAsync().ConfigureAwait(false);
  }
}
```

## GoodCitizen

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

## HostedServices

- Hosted Services are managed by the host and started and stopped (in reverse order) the host
- `IHostedService` is the basic abstraction. For long running background tasks use `BackgroundService`.
- GenericHost starts hosted services before everything else while WebHost starts the ASP.NET Core pipeline concurrent to the hosted services. This means initializing things in hosted services that controllers rely on can be dangerous.

## Pipelines

- All buffer management is delegated to the `PipeReader`/`PipeWriter` implementations.`
- Besides handling the memory management, the other core pipelines feature is the ability to peek at data in the Pipe without actually consuming it.
- `FlushAsync` provides back pressure and flow control. PipeWriter.FlushAsync “blocks” when the amount of data in the Pipe crosses PauseWriterThreshold and “unblocks” when it becomes lower than ResumeWriterThreshold.
- `PipeScheduler` gives fine grained control over scheduling the IO.

## PowerfulAsyncEnumerable

- Async Enumerable can be combined in powerful ways with other async and TPL constructs such as `Task.WhenAny`

## ShortcutStatemachine

- For highperf scenario `async` keyword can be omitted
- Apply carefully and only after measuring
- For most scenarios apply the keyword since it prevents mistakes because
  - Asynchronous and synchronous exceptions are normalized to always be asynchronous.
  - The code is easier to modify (consider adding a using, for example).
  - Diagnostics of asynchronous methods are easier (debugging hangs etc).
  - Exceptions thrown will be automatically wrapped in the returned Task instead of surprising the caller with an actual exception.
- NET Core 2.2:

|  Method | Allocated |
|-------- |----------:|
|  Return |     376 B |
|  Await  |     488 B |

- NET Core 3.0:

|  Method | Allocated |
|-------- |----------:|
|  Return |     344 B |
|  Await  |     456 B |


## StackTracesOhMy

- With .NET Core 2.1 and later finally readable stack traces
```
   at StackTracesOhMy.Level6() in C:\p\Async.Netcore\StackTracesOhMy.cs:line 19
   at StackTracesOhMyExtensions.Level5(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 41
   at StackTracesOhMyExtensions.Level4(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 36
   at StackTracesOhMyExtensions.Level3(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 31
   at StackTracesOhMyExtensions.Level2(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 26
   at StackTracesOhMyExtensions.Level2to6(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 21
```

## SyncOverAsync

- `Task.Result` or `Task.Wait` on asynchronous operations is much worse than calling truly synchronous APIs. Here is what happens
  - An asynchronous operation is kicked off. 
  - The calling thread is blocked waiting for that operation to complete.
  When the asynchronous operation completes, it unblocks the code waiting on that operation. This takes place on another thread.
- This leads to thread-pool starvation and service outages due to 2 threads being used instead of 1 to complete synchronous operations.
- If a synchronization context is available it can even lead to deadlocks

## UnobservedException

- Async void methods will crash the process if an exception is thrown
- `Task`-returning methods are better since unhandled exceptions trigger the `TaskScheduler.UnobservedTaskException`
- Be ware that only when the finalizers are run the unobserved exception is thrown

## ValueTasks

- Nice for highperf scenarios and only then!
- It's not about replacing Task
- Has a single purpose: Reduce heap allocations on the hot path where common synchronous execution is possible
- Do not
 - Await the instance multiple times
 - Call `AsTask` multiple times
 - Use `.Result` or `.GetAwaiter().GetResult()` when not yet completed
 - Use more than one of these techniques to consume the instance
- Complex to use and easy to get wrong
- Stats:

|                   Method | Repeats |        Mean |       Error |      StdDev |      Median | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------- |------------:|------------:|------------:|------------:|------:|--------:|--------:|------:|------:|----------:|
|              ConsumeTask |    1000 | 13,068.1 ns | 1,537.87 ns | 4,437.11 ns | 10,850.4 ns |  3.08 |    0.95 | 17.2119 |     - |     - |   72072 B |
|    ConsumeValueTaskWrong |    1000 | 13,884.2 ns |   549.64 ns | 1,523.06 ns | 13,473.2 ns |  3.11 |    0.50 |       - |     - |     - |         - |
| ConsumeValueTaskProperly |    1000 |  4,567.8 ns |    90.81 ns |   133.11 ns |  4,543.2 ns |  1.00 |    0.00 |       - |     - |     - |         - |
|    ConsumeValueTaskCrazy |    1000 |  3,380.4 ns |    67.19 ns |    74.69 ns |  3,371.6 ns |  0.74 |    0.03 |       - |     - |     - |         - |     

https://github.com/adamsitnik/StateOfTheDotNetPerformance        

