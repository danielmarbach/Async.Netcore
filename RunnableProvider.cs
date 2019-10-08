static class RunnableProvider
{
    public static IRunnable[] All() => new IRunnable[] {
        new SyncOverAsync(), new GoodCitizen(), new ShortcutStatemachine(), new StackTracesOhMy(),
        new AsyncDisposable(), new AsyncEnumerable(), new CustomAsyncEnumerable(), new PowerfulAsyncEnumerable() };
}