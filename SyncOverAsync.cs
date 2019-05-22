using System.Threading.Tasks;

class SyncOverAsync : IRunnable
{
    public Task Run()
    {
        return this.WrapInContext(() =>
        {
            //BlocksAndPotentiallyDeadlocks();
            //BlocksAndPotentiallyDeadlocks2();
        });
    }

    string BlocksUsesDefaultSchedulerThrowsAggregateException()
    {
        return Task.Run(() => this.DoAsyncOperation()).Result;
    }

    string BlocksUsesDefaultScheduler()
    {
        return Task.Run(() => this.DoAsyncOperation()).GetAwaiter().GetResult();
    }

    string BlocksUsesDefaultSchedulerThrowsAggregateExceptionWithAggregateException()
    {
        return Task.Run(() => this.DoAsyncOperation().Result).Result;
    }

    string BlocksThreadThatEntersAndThreadPoolThreadInside()
    {
        return Task.Run(() => this.DoAsyncOperation().GetAwaiter().GetResult()).GetAwaiter().GetResult();
    }

    string BlocksCapturesContextPotentiallyDeadlocksThrowsAggregateException()
    {
        return this.DoAsyncOperation().Result;
    }

    string BlocksAndPotentiallyDeadlocks()
    {
        return this.DoAsyncOperation().GetAwaiter().GetResult();
    }

    string BlocksAndPotentiallyDeadlocks2()
    {
        var task = this.DoAsyncOperation();
        task.Wait(1000); // timeout deliberately added
        return task.GetAwaiter().GetResult();
    }
}
