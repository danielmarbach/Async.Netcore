## SyncOverAsync

- `Task.Result` or `Task.Wait` on asynchronous operations is much worse than calling truly synchronous APIs. Here is what happens
  - An asynchronous operation is kicked off. 
  - The calling thread is blocked waiting for that operation to complete.
  When the asynchronous operation completes, it unblocks the code waiting on that operation. This takes place on another thread.
- This leads to thread-pool starvation and service outages due to 2 threads being used instead of 1 to complete synchronous operations.
- If a synchronization context is available it can even lead to deadlocks

## GoodCitizen

- Is async all the way and embraces virality of the API where it makes sense (IO-bound path)
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

## UnobservedException

- Only when the finalizers are run the unobserved exception is thrown

## CompletedTask

- For pre-computed results, there's no need to call `Task.Run`, that will end up queuing a work item to the thread pool that will immediately complete with the pre-computed value. Instead, use `Task.FromResult`, to create a task wrapping already computed data.
- Alternatively `ValueTask<TResult>` can be used if low allocations are desired.

## ShortcutStatemachine

- For highperf scenario `async` keyword can be omitted
- Apply carefully and only after measuring
- For most scenarios apply the keyword since it prevents mistakes because
  - Asynchronous and synchronous exceptions are normalized to always be asynchronous.
  - The code is easier to modify (consider adding a using, for example).
  - Diagnostics of asynchronous methods are easier (debugging hangs etc).
  - Exceptions thrown will be automatically wrapped in the returned Task instead of surprising the caller with an actual exception.
- NET Core 2.0:

 |  Method |      Mean |     Error |    StdDev | Scaled | Allocated |
 |-------- |----------:|----------:|----------:|-------:|----------:|
 |  Return | 15.576 ms | 0.4185 ms | 0.0236 ms |   1.00 |     528 B |
 |  Simple | 15.568 ms | 0.8275 ms | 0.0468 ms |   1.00 |     744 B |
 | Actions |  2.008 ms | 0.0756 ms | 0.0043 ms |   0.13 |     560 B |

- NET Core 2.1:

 |  Method |      Mean |     Error |    StdDev | Scaled | Allocated |
 |-------- |----------:|----------:|----------:|-------:|----------:|
 |  Return | 15.542 ms | 1.3313 ms | 0.0752 ms |   1.00 |     376 B |
 |  Simple | 15.538 ms | 1.4433 ms | 0.0815 ms |   1.00 |     488 B |
 | Actions |  1.939 ms | 0.4590 ms | 0.0259 ms |   0.12 |     350 B |


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

## ValueTasks

- Nice for highperf scenarios and only then!
- Complex to use and easy to get wrong
- Stats:

 |                   Method | Repeats |        Mean |      Error |       StdDev |      Median | Scaled | ScaledSD |   Gen 0 | Allocated |
 |------------------------- |-------- |------------:|-----------:|-------------:|------------:|-------:|---------:|--------:|----------:|
 |          **ConsumeTask** |    1000 |  9,307.1 ns | 396.345 ns | 1,091.649 ns |  9,501.1 ns |   2.00 |     0.60 | 11.4441 |   72072 B |
 |    ConsumeValueTaskWrong |    1000 | 11,073.7 ns | 468.996 ns | 1,382.844 ns | 10,329.0 ns |   2.38 |     0.73 |       - |       0 B |
 | ConsumeValueTaskProperly |    1000 |  5,075.2 ns | 543.450 ns | 1,602.374 ns |  4,455.4 ns |   1.00 |     0.00 |       - |       0 B |
 |    ConsumeValueTaskCrazy |    1000 |  4,140.6 ns | 211.741 ns |   604.109 ns |  4,201.2 ns |   0.89 |     0.28 |       - |       0 B |        

https://github.com/adamsitnik/StateOfTheDotNetPerformance        

## AsyncDisposable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

## AsyncEnumerable

- `IAsyncEnumerable<T>` allows to write asynchronous pull based streams, similar to regular enumerables with `yield return` and `yield break`
- `WithCancellation` only adds the token to the enumerator but doesn't influence the state machine
- `WithCancellation` in combination with `[EnumeratorCancellation]` can be used to ceate a combined token

## DefaultInterfaces

- Finally we can evolve interfaces as well.
- They can use the regular keywords.
- The compiler generates (of course with the async statemachine that I omitted here)
```
public interface IRunV2 : IRun
{
  async Task Run()
  {
    await this.RunAsync().ConfigureAwait(false);
  }
}
```

## HostedServices

- Hosted Services are managed by the host and started and stopped (in reverse order) the host
- `IHostedService` is the basic abstraction. For long running background tasks use `BackgroundService`.
- GenericHost starts hosted services before everything else while WebHost starts the ASP.NET Core pipeline concurrent to the hosted services. This means initializing things in hosted services that controllers rely on can be dangerous.

## Pipelines

- All buffer management is delegated to the `PipeReader`/`PipeWriter` implementations.`
- Besides handling the memory management, the other core pipelines feature is the ability to peek at data in the Pipe without actually consuming it.
- `FlushAsync` provides back pressure and flow control. PipeWriter.FlushAsync “blocks” when the amount of data in the Pipe crosses PauseWriterThreshold and “unblocks” when it becomes lower than ResumeWriterThreshold.
- `PipeScheduler` gives fine grained control over scheduling the IO.

## Channels

- Low-level async primitives that allows to build higher level primitives like Dataflow library for example.
- Influenced by Go channels
- Data structure for publish/subscribe scenarios
- Allows decoupling of publishes from subscribers
- Can be combined in-memory or combined with pipelines to buffer network input and output
- Very good overview can be found in [Nicolas Portmann's article](https://ndportmann.com/system-threading-channels/)

