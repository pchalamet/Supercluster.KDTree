namespace Workbench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using KDTree;

    using KDTreeTests;

    using Supercluster.KDTree;

    class Program
    {
        static void Main(string[] args)
        {
            TreeSpeedTests();
        }

        public static T Test<T>()
        {
            var type = typeof(T);
            return (T)type.GetField("MaxValue").GetValue(type);
        }


        static void SortedArraySpeedTests()
        {
            var dataSize = 1000000;
            var range = 1000;
            var capacity = 10;

            var treeData = Utilities.GenerateFloats(dataSize, range).ToArray();
            var nnList = new BoundedPriorityList<float, float>(capacity);

            var samples1 = new List<long>();
            var samples2 = new List<long>();

            for (int i = 0; i < 200; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (var f in treeData)
                {
                    nnList.Add(f[0], f[1]);
                }
                stopwatch.Stop();
                samples1.Add(stopwatch.ElapsedTicks);
            }

            var statistic = CalculateStatistics(samples1, samples2);
            Console.WriteLine("Method1 Average: " + statistic.Method1Average);
            Console.WriteLine("Method2 Average: " + statistic.Method2Average);
            Console.WriteLine("T-Statistic: " + statistic.TStatistic);
            Console.WriteLine("Statistic should be greater than or less than +/- 1.645 for significance.");
            Console.WriteLine("A negative statistics is better.\n");
            Console.ReadLine();

        }


        static void TreeSpeedTests()
        {
            var dataSize = 200000;
            var testDataSize = 1000;
            var range = 1000;
            var treeData = Utilities.GenerateFloats(dataSize, range);
            var testData = Utilities.GenerateFloats(testDataSize, range);
            var tree_new = new KDTree.KDTree<float>(2, treeData, Utilities.L2Norm_Squared_Float);

            var samples = new List<long>();
            for (int j = 0; j < 50; j++)
            {
                // Setup new tree

                var stopwatch_tree = new Stopwatch();
                stopwatch_tree.Start();
                for (int i = 0; i < testData.Length; i++)
                {
                    var test = tree_new.NearestNeighbors(testData[i], 1);
                }
                stopwatch_tree.Stop();
                samples.Add(stopwatch_tree.ElapsedTicks);
            }



            var stopwatch_linear = new Stopwatch();
            Console.WriteLine("Linear Search For Comparison " + DateTime.Now);
            stopwatch_linear.Start();
            for (int i = 0; i < testDataSize; i++)
            {
                var test = Utilities.LinearSearch(treeData, testData[i], Utilities.L2Norm_Squared_Float);
            }
            stopwatch_linear.Stop();
            Console.WriteLine("Linear ticks: " + stopwatch_linear.ElapsedTicks);
            // Console.ReadLine();



            var statistic = CalculateStatistics(samples, samples);
            Console.WriteLine("Method1 Average: " + statistic.Method1Average);
            Console.WriteLine("Method2 Average: " + statistic.Method2Average);
            Console.WriteLine("T-Statistic: " + statistic.TStatistic);
            Console.WriteLine("Statistic should be greater than or less than +/- 1.645 for significance.");
            Console.WriteLine("A negative statistics is better.\n");
            Console.ReadLine();
        }

        #region Statistics

        static PerformanceStatistics CalculateStatistics(List<long> samples1, List<long> samples2)
        {
            var samples = samples1.Count;
            var pooledVariance = Math.Sqrt((((samples - 1) * StandardDeviation(samples1) * StandardDeviation(samples1))
         + ((samples - 1) * StandardDeviation(samples2) * StandardDeviation(samples2))) / (2 * samples - 2));

            var T = (samples1.Average() - samples2.Average()) / (pooledVariance * Math.Sqrt(2.0 / samples));

            return new PerformanceStatistics
            {
                Method1Samples = samples1,
                Method1Average = samples1.Average(),
                Method2Samples = samples2,
                Method2Average = samples2.Average(),
                TStatistic = T
            };
        }

        static double StandardDeviation(IEnumerable<long> data)
        {
            var mean = data.Average();
            return Math.Sqrt(data.Sum(point => Math.Pow(point - mean, 2)) / (data.Count() - 1));
        }

        public class PerformanceStatistics
        {
            public List<long> Method1Samples { get; set; }
            public List<long> Method2Samples { get; set; }
            public double Method1Average { get; set; }
            public double Method2Average { get; set; }
            public double TStatistic { get; set; }
        }
        #endregion
    }


}
