using System;
using System.IO;
using System.Threading.Tasks;

static class StackTracesOhMyExtensions
{
    public static async Task PrintStackTrace(this StackTracesOhMy runnable, Func<Task> function)
    {
        try
        {
            await function();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
    }

    public static async Task Level2to6(this StackTracesOhMy runnable)
    {
        await Level2(runnable);
    }

    static async Task Level2(StackTracesOhMy runnable)
    {
        await Level3(runnable);
    }

    static async Task Level3(StackTracesOhMy runnable)
    {
        await Level4(runnable);
    }

    static async Task Level4(StackTracesOhMy runnable)
    {
        await Level5(runnable);
    }

    static async Task Level5(StackTracesOhMy runnable)
    {
        await runnable.Level6();
    }

    public static void Explain(this StackTracesOhMy runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- With .NET Core 2.1 and later finally readable stack traces
```
   at StackTracesOhMy.Level6() in C:\p\Async.Netcore\StackTracesOhMy.cs:line 19
   at StackTracesOhMyExtensions.Level5(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 41
   at StackTracesOhMyExtensions.Level4(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 36
   at StackTracesOhMyExtensions.Level3(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 31
   at StackTracesOhMyExtensions.Level2(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 26
   at StackTracesOhMyExtensions.Level2to6(StackTracesOhMy runnable) in C:\p\Async.Netcore\StackTracesOhMyExtensions.cs:line 21
```
");
    }    
}