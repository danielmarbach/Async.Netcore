using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

static class PipelinesExtensions
{
    public static void Explain(this Pipelines runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- All buffer management is delegated to the `PipeReader`/`PipeWriter` implementations.`
- Besides handling the memory management, the other core pipelines feature is the ability to peek at data in the Pipe without actually consuming it.
- `FlushAsync` provides back pressure and flow control. PipeWriter.FlushAsync “blocks” when the amount of data in the Pipe crosses PauseWriterThreshold and “unblocks” when it becomes lower than ResumeWriterThreshold.
- `PipeScheduler` gives fine grained control over scheduling the IO.
");
    }

    public static void ProcessLine(this Pipelines runnable, Socket socket, in ReadOnlySequence<byte> buffer)
    {
        Console.Write($"[{socket.RemoteEndPoint}]: ");
        foreach (var segment in buffer)
        {
            Console.Write(Encoding.UTF8.GetString(segment.Span));
        }
        Console.WriteLine();
    }

    public static void ReadUntilEOLAndOutputToConsole(this Pipelines runnable, Socket socket, ReadOnlySequence<byte> buffer)
    {
        SequencePosition? position = null;
        do
        {
            // Find the EOL
            position = buffer.PositionOf((byte)'\n');

            if (position != null)
            {
                var line = buffer.Slice(0, position.Value);
                runnable.ProcessLine(socket, line);

                // This is equivalent to position + 1
                var next = buffer.GetPosition(1, position.Value);

                // Skip what we've already processed including \n
                buffer = buffer.Slice(next);
            }
        }
        while (position != null);
    }

    public static async Task Send(this Pipelines runnable, CancellationTokenSource tokenSource)
    {
        var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8087));
        Console.WriteLine("Connecting to port 8087");

        string line;
        while ((line = Console.ReadLine()) != "exit")
        {
            var buffer = Encoding.ASCII.GetBytes(line.Replace(Environment.NewLine, string.Empty) + Environment.NewLine);

            await clientSocket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
        }

        tokenSource.Cancel();

        Console.WriteLine("Send done");
    }

    public static async Task Receive(this Pipelines runnable, Pipe pipe, CancellationToken token)
    {
        var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 8087));
        Console.WriteLine("Listening on port 8087");
        listenSocket.Listen(120);

        using (token.Register(() => listenSocket.Close()))
        {
            while (!token.IsCancellationRequested)
            {
                Socket socket;
                try
                {
                    socket = await listenSocket.AcceptAsync();
                }
                catch (SocketException)
                {
                    return;
                }
                _ = runnable.ProcessLinesAsync(socket, pipe, token);
            }
        }
    }
}
