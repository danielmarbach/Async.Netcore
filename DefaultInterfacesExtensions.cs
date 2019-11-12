using System.IO;

static class DefaultInterfacesExtensions {
        public static void Explain(this DefaultInterfaces runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Finally we can evolve interfaces as well.
- They can use the regular keywords.
- The compiler generates (of course with the async statemachine that I omitted here)
```
public interface IRun
{
  Task RunAsync();

  // added in v2
  async Task Run()
  {
    await this.RunAsync().ConfigureAwait(false);
  }
}
```
");
    }
}
