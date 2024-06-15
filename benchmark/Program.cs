using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<MyBenchmark>();

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[RankColumn]
public class MyBenchmark
{
    private Pessoa[] array;
    private List<Pessoa> list;

    [GlobalSetup]
    public void Setup()
    {
        var enumerable = Enumerable.Range(0, 1000).Select(x => new Pessoa());
        list = enumerable.ToList();
        array = enumerable.ToArray();
    }

    [Benchmark, BenchmarkCategory("Array")]
    public int ForArray()
    {
        int sum = 0;
        for (int i = 0; i < array.Length; i++)
        {
            var item = list[i];
            sum += item.Idade;
        }
        return sum;
    }

    [Benchmark, BenchmarkCategory("Array")]
    public int ForeachArray()
    {
        int sum = 0;
        foreach (var item in array)
        {
            sum += item.Idade;
        }
        return sum;
    }

    [Benchmark, BenchmarkCategory("List")]
    public int ForList()
    {
        int sum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            sum += item.Idade;
        }
        return sum;
    }

    [Benchmark, BenchmarkCategory("List")]
    public int ForeachList()
    {
        int sum = 0;
        foreach (var item in list)
        {
            sum += item.Idade;
        }
        return sum;
    }
}

public class Pessoa
{
    public int Idade { get; set; } = 1;
}