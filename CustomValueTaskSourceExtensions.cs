
using System;
using System.IO;

static class CustomValueTaskSourceExtensions
{
    public static void PrintResult(this CustomValueTaskSource runnable, long result)
    {
        Console.Write($"{result} ");
    }

    public static void Explain(this CustomValueTaskSource runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Very powerful tool to write cached task sources without dropping deep to the async machinery
- Mostly for framework and library authors
");
    }
}