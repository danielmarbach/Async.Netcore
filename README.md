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

## Pipelines

- All buffer management is delegated to the `PipeReader`/`PipeWriter` implementations.`
- Besides handling the memory management, the other core pipelines feature is the ability to peek at data in the Pipe without actually consuming it.
- `FlushAsync` provides back pressure and flow control. PipeWriter.FlushAsync “blocks” when the amount of data in the Pipe crosses PauseWriterThreshold and “unblocks” when it becomes lower than ResumeWriterThreshold.
- `PipeScheduler` gives fine grained control over scheduling the IO.

