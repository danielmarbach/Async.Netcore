using System.IO;

static class UnobservedExceptionExtensions
{
    public static void Explain(this UnobservedException runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Async void methods will crash the process if an exception is thrown
- `Task`-returning methods are better since unhandled exceptions trigger the `TaskScheduler.UnobservedTaskException`
- Be ware that only when the finalizers are run the unobserved exception is thrown
");
    }
}