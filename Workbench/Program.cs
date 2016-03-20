namespace Workbench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using KDTreeTests;

    using Supercluster.KDTree;

    class Program
    {
        public static double[][] GenerateData(int points, double range)
        {
            var data = new List<double[]>();
            var random = new Random();

            for (int i = 0; i < points; i++)
            {
                data.Add(new double[] { (random.NextDouble() * range), (random.NextDouble() * range) });
            }

            return data.ToArray();
        }

        static void Main(string[] args)
        {
            // Define the metric function
            Func<double[], double[], double> L2Norm = (x, y) =>
            {
                double dist = 0f;
                for (int i = 0; i < x.Length; i++)
                {
                    dist += (x[i] - y[i]) * (x[i] - y[i]);
                }

                return dist;
            };

            // Generate some data for the tree and testing
            var treeData = GenerateData(1000000, 1000);
            var testData = GenerateData(10000, 1000);
            var tree = new KDTree<double>(2, treeData, L2Norm);

            // Measure the time to search
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < testData.Length; i++)
            {
                var test = tree.RadialSearch(testData[i], 100, 3);
            }
            stopwatch.Stop();

            Console.WriteLine("Milliseconds: " + stopwatch.ElapsedMilliseconds);
            Console.Read();
        }
    }

}
