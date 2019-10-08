using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

static class AsyncDisposableExtensions
{
    public static void Explain(this AsyncDisposable runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- `Stream`, `Utf8JsonWriter`, `System.Threading.Timer`, `CancellationTokenRegistration`, `BinaryWriter`, `TextWriter` and `IAsyncEnumerator<T>` implement `IAsyncDisposable`
- `Stream`, `BinaryWriter`, `TextWriter`  calls `.Dispose` synchronously
- `Stream` `FlushAsync` calls `Flush` on another thread which is bad behavior that should be overwritten
");
    }
}

class DisposableBase
{
    protected Stream stream;

    protected DisposableBase()
    {
        stream = new FakeStream();
    }

    protected void Log([CallerMemberName] string member = null)
    {
        Console.WriteLine($"{GetType().Name}.{member}()");
    }

    class FakeStream : Stream
    {
        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            Thread.Sleep(1000);
        }

        public override async Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}