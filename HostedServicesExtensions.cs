using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Hosting;

static class HostedServicesExtensions
{
    public static void PrintStart(this HostedServices runnable)
    {
        Console.WriteLine($"Host on {Thread.CurrentThread.ManagedThreadId} starting at: {DateTime.Now}");
    }

    public static void PrintStop(this HostedServices runnable)
    {
        Console.WriteLine($"Host on {Thread.CurrentThread.ManagedThreadId} stopping at: {DateTime.Now}");
    }

    public static void PrintStart(this BackgroundService runnable)
    {
        Console.WriteLine($"Worker on {Thread.CurrentThread.ManagedThreadId} started at: {DateTime.Now}");
    }

    public static void PrintWorking(this BackgroundService runnable)
    {
        Console.WriteLine($"Worker {Thread.CurrentThread.ManagedThreadId} running at: {DateTime.Now}");
    }

    public static void PrintStopped(this BackgroundService runnable)
    {
        Console.WriteLine($"Worker {Thread.CurrentThread.ManagedThreadId} stopped at: {DateTime.Now}");
    }

    public static void PrintDisposed(this BackgroundService runnable)
    {
        Console.WriteLine($"Worker {Thread.CurrentThread.ManagedThreadId} disposed at: {DateTime.Now}");
    }

    public static void Explain(this HostedServices runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Hosted Services are managed by the host and started and stopped (in reverse order) the host
- `IHostedService` is the basic abstraction. For long running background tasks use `BackgroundService`.
- GenericHost starts hosted services before everything else while WebHost starts the ASP.NET Core pipeline concurrent to the hosted services. This means initializing things in hosted services that controllers rely on can be dangerous.
");
    }
}
