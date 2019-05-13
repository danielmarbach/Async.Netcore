using System;
using System.Threading.Tasks;

class CompletedTask : IRunnable
{
    public async Task Run()
    {
        var calculator1 = new BadCalculator1();
        Console.WriteLine(await calculator1.Calculate(2,2));

        var calculator2 = new BadCalculator2();
        Console.WriteLine(await calculator2.Calculate(2,2));

        var calculator3 = new GoodCalculator();
        Console.WriteLine(await calculator3.Calculate(2,2));

        var calculator4 = new EfficientCalculator();
        Console.WriteLine(await calculator4.Calculate(2,2));
    }

    class BadCalculator1 {
        public Task<int> Calculate(int a, int b) {
            return Task.Run(() => (a+b));
        }
    }

    class BadCalculator2 {
        public async Task<int> Calculate(int a, int b) {
            await Task.Yield();
            return a+b;
        }
    }

    class GoodCalculator {
        public Task<int> Calculate(int a, int b) {
            return Task.FromResult(a+b);
        }
    }

    class EfficientCalculator {
        public ValueTask<int> Calculate(int a, int b) {
            return new ValueTask<int>(a+b);
        }
    }
}
