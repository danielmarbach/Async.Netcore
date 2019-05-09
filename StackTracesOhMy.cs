using System;
using System.Threading.Tasks;

class StackTracesOhMy : IRunnable
{
    public async Task Run()
    {
        await this.PrintStackTrace(async () => await Level1());
    }

    public async Task Level1()
    {
        await this.Level2to6();
    }

    public async Task Level6()
    {
        await Task.Yield();
        throw new Exception("boom");
    }
}
