## AsyncDisposable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

## AsyncEnumerable

- `IAsyncEnumerable<T>` allows to write asynchronous pull based streams, similar to regular enumerables with `yield return` and `yield break`
- `WithCancellation` only adds the token to the enumerator but doesn't influence the state machine
- `WithCancellation` in combination with `[EnumeratorCancellation]` can be used to ceate a combined token

## Channels

- 

## CustomValueTaskSource

- Very powerful tool to write cached task sources without dropping deep to the async machinery
- Mostly for framework and library authors

## Pipelines

- All buffer management is delegated to the `PipeReader`/`PipeWriter` implementations.`
- Besides handling the memory management, the other core pipelines feature is the ability to peek at data in the Pipe without actually consuming it.
- `FlushAsync` provides back pressure and flow control. PipeWriter.FlushAsync “blocks” when the amount of data in the Pipe crosses PauseWriterThreshold and “unblocks” when it becomes lower than ResumeWriterThreshold.
- `PipeScheduler` gives fine grained control over scheduling the IO.

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

## UnobservedException

- Only when the finalizers are run the unobserved exception is thrown

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

