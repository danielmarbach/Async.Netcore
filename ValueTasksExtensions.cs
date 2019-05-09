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
- Complex to use and easy to get wrong
- Stats:

 |                   Method | Repeats |        Mean |      Error |       StdDev |      Median | Scaled | ScaledSD |   Gen 0 | Allocated |
 |------------------------- |-------- |------------:|-----------:|-------------:|------------:|-------:|---------:|--------:|----------:|
 |          **ConsumeTask** |    1000 |  9,307.1 ns | 396.345 ns | 1,091.649 ns |  9,501.1 ns |   2.00 |     0.60 | 11.4441 |   72072 B |
 |    ConsumeValueTaskWrong |    1000 | 11,073.7 ns | 468.996 ns | 1,382.844 ns | 10,329.0 ns |   2.38 |     0.73 |       - |       0 B |
 | ConsumeValueTaskProperly |    1000 |  5,075.2 ns | 543.450 ns | 1,602.374 ns |  4,455.4 ns |   1.00 |     0.00 |       - |       0 B |
 |    ConsumeValueTaskCrazy |    1000 |  4,140.6 ns | 211.741 ns |   604.109 ns |  4,201.2 ns |   0.89 |     0.28 |       - |       0 B |        

https://github.com/adamsitnik/StateOfTheDotNetPerformance        
");        
    }
}
