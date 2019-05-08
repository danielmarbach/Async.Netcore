
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

class AsyncEnumerable : IRunnable
{
    public async Task Run()
    {
        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(3200);
        
        await foreach (var delay in ReadDelays()
            .WithCancellation(tokenSource.Token)
            .ConfigureAwait(false))
        {
            Console.WriteLine($"await Task.Delay({delay})");

            await Task.Delay(delay)
                .ConfigureAwait(false);
        }
    }

    async IAsyncEnumerable<int> ReadDelays([EnumeratorCancellation] CancellationToken token = default)
    {
        using (var stream = File.OpenRead("AsyncEnumerable.txt"))
        using (var reader = new StreamReader(stream))
        {
            string line;
            while ((line = await reader.ReadLineAsync()
                .ConfigureAwait(false)) != null)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine("  Read next line");
                yield return Convert.ToInt32(line);
            }
        }
        Console.WriteLine("  Done enumerating");
    }
}
