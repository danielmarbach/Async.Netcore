using System.IO;

static class UnobservedExceptionExtensions
{
    public static void Explain(this UnobservedException runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Only when the finalizers are run the unobserved exception is thrown
");
    }
}