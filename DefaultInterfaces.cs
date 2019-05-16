using System;
using System.Threading.Tasks;

class DefaultInterfaces : IRunnable
{
    public async Task Run()
    {
        IRunV2 run = new Run();
        await run.Run();
    }
}

public interface IRun {
    Task RunAsync();
}

public interface IRunV2 : IRun {
    async Task Run() 
    {
        await RunAsync().ConfigureAwait(false);
    }
}

class Run : IRunV2
{
    public Task RunAsync()
    {
        Console.WriteLine("Hello from RunAsync");
        return Task.CompletedTask;
    }
}