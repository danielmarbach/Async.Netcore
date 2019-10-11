using System;
using System.Threading;
using System.Threading.Tasks;

class ShortcutStatemachine : IRunnable
{
    public async Task Run()
    {
        await this.PrintStackInformation(DoesNotShortcut);

        await this.PrintStackInformation(DoesShortcut);

        Surprise().Ignore();
    }

    async Task DoesNotShortcut()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
    }

    Task DoesShortcut()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        return Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
    }

    Task Surprise() 
    {
        throw new InvalidOperationException();
    }
}