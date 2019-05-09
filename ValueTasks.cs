using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class ValueTasks : IRunnable
{
    public Task Run()
    {
        return this.LoopTenTimesAndSumResult(async i =>
        {
            ValueTask<int> valueTask = Get("Foo");

            if (valueTask.IsCompleted)
            {
                this.PrintFastPath(i);
                return valueTask.Result;
            }
            else
            {
                this.PrintAsyncPath(i);
                return await valueTask.AsTask();
            }
        });
    }

    ValueTask<int> Get(string key)
    {
        if (cachedValues.TryGetValue(key, out int value))
        {
            return new ValueTask<int>(value);
        }

        return new ValueTask<int>(this.LoadFromFileAndCache(key));
    }

    internal ConcurrentDictionary<string, int> cachedValues = new ConcurrentDictionary<string, int>();
}
