using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

sealed class CustomTaskSource : IValueTaskSource
{
    private readonly int every;
    private long counter;

    public CustomTaskSource(int every)
    {
        this.every = every;
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        var increment = Interlocked.Increment(ref counter);
        if (increment % every == 0)
        {
            Console.Write("Scheduling | ");
            return ValueTaskSourceStatus.Pending;
        }
        Console.Write("Cached | ");
        return ValueTaskSourceStatus.Succeeded;
    }

    public void OnCompleted(Action<object> continuation, object state, short token,
        ValueTaskSourceOnCompletedFlags flags)
    {
        Console.Write("OnCompleted | ");
        Task.Delay(Convert.ToInt32(Interlocked.Read(ref counter)) * 500)
            .ConfigureAwait(flags == ValueTaskSourceOnCompletedFlags.UseSchedulingContext)
            .GetAwaiter()
            .OnCompleted(() =>
            {
                continuation(state);
                Console.WriteLine("Continuation | ");
            });
    }

    public void GetResult(short token)
    {
    }
}