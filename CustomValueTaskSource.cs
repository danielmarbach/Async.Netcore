
using System;
using System.Threading.Tasks;

class CustomValueTaskSource : IRunnable
{
    public async Task Run()
    {
        var valueTask = new ValueTask(new CustomTaskSource(3), 1);
        for (var i = 0; i < 7; i++)
        {
            await valueTask;
        }

        Console.WriteLine();
        Console.WriteLine("Custom source with result");
        var longValueTask = new ValueTask<long>(new CustomLongTaskSource(3), 1);
        for (var i = 0; i < 7; i++)
        {
            this.PrintResult(await longValueTask);
        }
        
    }
}