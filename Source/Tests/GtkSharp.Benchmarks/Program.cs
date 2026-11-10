using BenchmarkDotNet.Running;
using GtkSharp.Benchmarks;

BenchmarkRunner.Run<BindingBenchmarks>(args: args);
