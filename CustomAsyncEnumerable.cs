
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class CustomAsyncEnumerable : IRunnable
{
    public async Task Run()
    {
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
            if (called++ == 1)
                return new ValueTask<bool>(true);
            return new ValueTask<bool>(false);
        }
    }
}
