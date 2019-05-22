using System;
using System.Threading.Tasks;

static class TaskExtensions
{
    public static async Task IgnoreCancellation(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static void Ignore(this Task task) {}
}
