
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
        using var anotherTokenSource = new CancellationTokenSource();
        //anotherTokenSource.CancelAfter(2400);
        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(3200);
        
        await foreach (var delay in ReadDelays(anotherTokenSource.Token)
            .WithCancellation(tokenSource.Token) // calls GetAsyncEnumerator(token)
            .ConfigureAwait(false))
        {
            Console.WriteLine($"await Task.Delay({delay})");

            await Task.Delay(delay)
                .ConfigureAwait(false);
        }

        using var yetAnotherTokenSource = new CancellationTokenSource();
        var custom = new CustomEnumerable(yetAnotherTokenSource);
        await foreach (var delay in custom
            .WithCancellation(yetAnotherTokenSource.Token)
            .ConfigureAwait(false))
        {
            Console.WriteLine($"await Task.Delay({delay})");

            await Task.Delay(delay)
                .ConfigureAwait(false);
        }

        custom = new CustomEnumerable(yetAnotherTokenSource);
        await foreach (var delay in custom)
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
                .ConfigureAwait(false)) != null && !token.IsCancellationRequested)
            {
                Console.WriteLine("  Read next line");
                yield return Convert.ToInt32(line);
            }
        }
        Console.WriteLine("  Done enumerating");
    }

    class CustomEnumerable : IAsyncEnumerable<int>, IAsyncEnumerator<int>
    {
        int called = 0;
        private readonly CancellationTokenSource tokenSource;

        public CustomEnumerable(CancellationTokenSource tokenSource)
        {
            this.tokenSource = tokenSource;
        }
        public int Current => 1;

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Is token passed to WithCancellation? {this.tokenSource.Token == cancellationToken}");
            return this;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            if(called++ == 1)
                return new ValueTask<bool>(true);
            return new ValueTask<bool>(false);
        }
    }
}
