using System;
using System.Threading.Channels;
using System.Threading.Tasks;

class Channels : IRunnable
{
    public async Task Run()
    {
        var unboundedChannelOptions = new UnboundedChannelOptions();
        unboundedChannelOptions.SingleWriter = true;

        var channel = Channel.CreateUnbounded<int>(unboundedChannelOptions);
        var channelWriter = channel.Writer;

        await Task.WhenAll(Task.Run(() => Produce(channelWriter, 1000)), Task.Run(() => Read(channel.Reader, "R1")), Task.Run(() => Read(channel.Reader, "R2")));
    }

    async Task Read(ChannelReader<int> reader, string readerName)
    {
        while(await reader.WaitToReadAsync()) 
        {
            var value = await reader.ReadAsync();
            Console.Write($" {readerName}-{value} ");
        }
    }

    async Task Produce(ChannelWriter<int> writer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await writer.WaitToWriteAsync();
            await writer.WriteAsync(i).ConfigureAwait(false);
        }
        writer.Complete();
    }
}
