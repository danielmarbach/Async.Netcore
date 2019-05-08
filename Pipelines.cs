using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Pipelines : IRunnable
{
    public async Task Run()
    {
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));

        var receiveTask = this.Receive(pipe, token);
        var sendTask = this.Send(tokenSource);

        await Task.WhenAll(receiveTask, sendTask);
    }

    public async Task ProcessLinesAsync(Socket socket, Pipe pipe, CancellationToken token)
    {
        Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

        Task writing = FillPipeAsync(socket, pipe.Writer, token);
        Task reading = ReadPipeAsync(socket, pipe.Reader, token);

        await Task.WhenAll(reading, writing).IgnoreCancellation();

        Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
    }

    private static async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken token)
    {
        const int minimumBufferSize = 512;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Request a minimum of 512 bytes from the PipeWriter
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, token);
                if (bytesRead == 0)
                {
                    break;
                }

                // Tell the PipeWriter how much was read
                writer.Advance(bytesRead);
            }
            catch
            {
                break;
            }

            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync(token);

            if (result.IsCompleted)
            {
                break;
            }
        }

        // Signal to the reader that we're done writing
        writer.Complete();
    }

    async Task ReadPipeAsync(Socket socket, PipeReader reader, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ReadResult result = await reader.ReadAsync(token);

            ReadOnlySequence<byte> buffer = result.Buffer;

            this.ReadUntilEOLAndOutputToConsole(socket, buffer);

            // We sliced the buffer until no more data could be processed
            // Tell the PipeReader how much we consumed and how much we left to process
            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        reader.Complete();
    }


}
