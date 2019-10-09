## SyncOverAsync

- `Task.Result` or `Task.Wait` on asynchronous operations is much worse than calling truly synchronous APIs. Here is what happens
  - An asynchronous operation is kicked off. 
  - The calling thread is blocked waiting for that operation to complete.
  When the asynchronous operation completes, it unblocks the code waiting on that operation. This takes place on another thread.
- This leads to thread-pool starvation and service outages due to 2 threads being used instead of 1 to complete synchronous operations.
- If a synchronization context is available it can even lead to deadlocks  

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

- NET Core 3.0 RC1:

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

 ## AsyncDisposable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

## AsyncEnumerable

- `IAsyncEnumerable<T>` allows to write asynchronous pull based streams, similar to regular enumerables with `yield return` and `yield break`
- `WithCancellation` only adds the token to the enumerator but doesn't influence the state machine
- `WithCancellation` in combination with `[EnumeratorCancellation]` can be used to create a combined token

## CustomAsyncEnumerable

- `WithCancellation` instructs the compiler generated statemachine to call `GetAsyncEnumerator` with the provided token. Otherwise `default(CancellationToken)` will be used.

## PowerfulAsyncEnumerable

- Async Enumerable can be combined in powerful ways with other async and TPL constructs such as `Task.WhenAny`
