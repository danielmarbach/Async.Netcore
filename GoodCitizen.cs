using System;
using System.Threading;
using System.Threading.Tasks;

class GoodCitizen : IRunnable
{
    public async Task Run()
    {
        using var tokenSource = new CancellationTokenSource();
        this.PrintStart();
        await Task.Delay(TimeSpan.FromDays(1))
            .WithCancellation(tokenSource.Token)
            .IgnoreCancellation();
        this.PrintEnd();

        this.PrintStart();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        await Task.Delay(TimeSpan.FromDays(1))
            .WithCancellation(tokenSource.Token)
            .IgnoreCancellation();
        this.PrintEnd();
    }
}

static class GoodCitizenTaskExtensions
{
    public static async Task WithCancellation(this Task task, CancellationToken token = default)
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(10));

        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = linkedTokenSource.Token.Register(state =>
        {
            ((TaskCompletionSource<object>)state).TrySetResult(null);
        }, tcs, useSynchronizationContext: false);

        var resultTask = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
        if (resultTask == tcs.Task)
        {
            // Operation cancelled
            throw new OperationCanceledException(token.IsCancellationRequested ? token : linkedTokenSource.Token);
        }

        await task.ConfigureAwait(false);
    }
}
