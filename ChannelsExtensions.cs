using System.IO;

static class ChannelsExtensions {
    public static void Explain(this Channels runnable, TextWriter writer) {
              writer.WriteLine(@"
- Low-level async primitives that allows to build higher level primitives like Dataflow library for example.
- Influenced by Go channels
- Data structure for publish/subscribe scenarios
- Allows decoupling of publishes from subscribers
- Can be combined in-memory or combined with pipelines to buffer network input and output
");  
    }
}
