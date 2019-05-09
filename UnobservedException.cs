
using System;
using System.Threading.Tasks;

class UnobservedException : IRunnable
{
    public async Task Run()
    {
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        ThrowUnobservedException();

        await Task.Delay(2000);

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine("Unobserved exception!");
        Console.WriteLine(e.Exception);
        e.SetObserved();
    }

    static async Task ThrowUnobservedException()
    {
        throw new Exception();
    }
}