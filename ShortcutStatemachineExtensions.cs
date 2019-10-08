using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

static class ShortcutStatemachineExtensions
{
    public static async Task PrintStackInformation(this ShortcutStatemachine runnable, Func<Task> method)
    {
        try
        {
            await method().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            var stackTrace = new StackTrace(1, true);

            Console.WriteLine($"{method.Method.Name}: FrameCount {stackTrace.FrameCount} / Has AsyncMethodBuilder '{stackTrace.ToString().Contains("AsyncTaskMethodBuilder")}'");
        }
    }

    public static void Explain(this ShortcutStatemachine runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- For highperf scenario `async` keyword can be omitted
- Apply carefully and only after measuring
- For most scenarios apply the keyword since it prevents mistakes because
  - Asynchronous and synchronous exceptions are normalized to always be asynchronous.
  - The code is easier to modify (consider adding a using, for example).
  - Diagnostics of asynchronous methods are easier (debugging hangs etc).
  - Exceptions thrown will be automatically wrapped in the returned Task instead of surprising the caller with an actual exception.
- NET Core 2.2:

|  Method | Allocated |
|-------- |----------:|
|  Return |     376 B |
|  Await  |     488 B |

- NET Core 3.0 RC1:

|  Method | Allocated |
|-------- |----------:|
|  Return |     344 B |
|  Await  |     456 B |

");
    }
}