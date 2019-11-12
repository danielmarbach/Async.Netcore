using System;
using System.Threading.Tasks;

class DefaultInterfaces : IRunnable
{
    public async Task Run()
    {
        IRun run = new Run();
        await run.Run();
    }
}

public interface IRun {
    Task RunAsync();

    // Method added in V2
    async ValueTask Run() 
    {
        await RunAsync().ConfigureAwait(false);
    }
}

class Run : IRun
{
    public Task RunAsync()
    {
        Console.WriteLine("Hello from RunAsync");
        return Task.CompletedTask;
    }
}