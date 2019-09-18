using System;
using System.IO;
using System.Threading.Tasks;

static class ValueTasksExtensions
{
    public static async Task LoopTenTimesAndSumResult(this ValueTasks runnable, Func<int, Task<int>> action)
    {
        int total = 0;
        for (int i = 0; i < 10; i++)
        {
            total += await action(i);
        }
        Console.WriteLine($"Result {total}");
    }

    public static async Task<int> LoadFromFileAndCache(this ValueTasks runnable, string key)
    {
        using (var stream = File.OpenText(@"Values.txt"))
        {
            string line;
            while ((line = await stream.ReadLineAsync()) != null)
            {
                var splitted = line.Split(Convert.ToChar(";"));
                var k = splitted[0];
                var v = Convert.ToInt32(splitted[1]);

                if (k != key)
                {
                    continue;
                }

                runnable.cachedValues.TryAdd(k, v);
                return v;
            }
        }
        return 0;
    }

    public static void PrintFastPath(this ValueTasks runnable, int i)
    {
        Console.WriteLine($"Fast path {i}.");
    }

    public static void PrintAsyncPath(this ValueTasks runnable, int i)
    {
        Console.WriteLine($"Async path {i}.");
    }

    public static void Explain(this ValueTasks runnable, TextWriter writer)
    {
        writer.WriteLine(@"
- Nice for highperf scenarios and only then!
- It's not about replacing Task
- Has a single purpose: Reduce heap allocations on the hot path where common synchronous execution is possible
- Do not
 - Await the instance multiple times
 - Call `AsTask` multiple times
 - Use `.Result` or `.GetAwaiter().GetResult()` when not yet completed
 - Use more than one of these techniques to consume the instance
- Complex to use and easy to get wrong
- Stats:

|                   Method | Repeats |        Mean |       Error |      StdDev |      Median | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------- |------------:|------------:|------------:|------------:|------:|--------:|--------:|------:|------:|----------:|
|              ConsumeTask |    1000 | 13,068.1 ns | 1,537.87 ns | 4,437.11 ns | 10,850.4 ns |  3.08 |    0.95 | 17.2119 |     - |     - |   72072 B |
|    ConsumeValueTaskWrong |    1000 | 13,884.2 ns |   549.64 ns | 1,523.06 ns | 13,473.2 ns |  3.11 |    0.50 |       - |     - |     - |         - |
| ConsumeValueTaskProperly |    1000 |  4,567.8 ns |    90.81 ns |   133.11 ns |  4,543.2 ns |  1.00 |    0.00 |       - |     - |     - |         - |
|    ConsumeValueTaskCrazy |    1000 |  3,380.4 ns |    67.19 ns |    74.69 ns |  3,371.6 ns |  0.74 |    0.03 |       - |     - |     - |         - |     

https://github.com/adamsitnik/StateOfTheDotNetPerformance        
");        
    }
}
