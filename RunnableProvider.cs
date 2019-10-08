static class RunnableProvider
{
    public static IRunnable[] All() => new IRunnable[] {
        new SyncOverAsync(), new GoodCitizen(), new UnobservedException(), new CompletedTask(), new ShortcutStatemachine(), new StackTracesOhMy(),
        new ValueTasks(), new AsyncDisposable(), new AsyncEnumerable(), new CustomAsyncEnumerable(), new PowerfulAsyncEnumerable(), new DefaultInterfaces() };
}