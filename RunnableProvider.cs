using System;
using System.Linq;

static class RunnableProvider
{
    public static IRunnable[] Presentation() => new IRunnable[] {
        new SyncOverAsync(), new GoodCitizen(), new ShortcutStatemachine(), new StackTracesOhMy(),
        new AsyncDisposable(), new AsyncEnumerable(), new CustomAsyncEnumerable(), new PowerfulAsyncEnumerable() };


    public static IRunnable[] All() => (from type in typeof(Program).Assembly.GetTypes()
                                        where typeof(IRunnable).IsAssignableFrom(type) && type != typeof(IRunnable)
                                        orderby type.Name
                                        select (IRunnable)Activator.CreateInstance(type)).ToArray();
}