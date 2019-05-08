## AsyncDisposable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

## AsyncEnumerable

- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten

