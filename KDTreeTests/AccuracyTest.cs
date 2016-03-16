namespace KDTreeTests
{

    using NUnit.Framework;
    using static KDTree.BinaryTreeUtilities;

    [TestFixture]
    public class AccuracyTest
    {

        /// <summary>
        /// Should build the tree displayed in the article:
        /// https://en.wikipedia.org/wiki/K-d_tree
        /// </summary>
        [Test]
        public void WikipediaBuildTests()
        {
            // Should generate the following tree:
            //             7+2
            //              |
            //       +------+-----+
            //      5,4          9,6
            //       +            +
            //   +---+---+     +--+
            //  2,3     4,7   8,1 


            var points = new double[][]
                             {
                                 new double[] { 7, 2 },
                                 new double[] { 5, 4 },
                                 new double[] { 2, 3 },
                                 new double[] { 4, 7 },
                                 new double[] { 9, 6 },
                                 new double[] { 8, 1 }
                             };


            var tree = new KDTree.KDTree<double>(2, double.MinValue, double.MaxValue, Utilities.L2Norm_Squared_Double, points);

            Assert.That(tree.InternalArray[tree.Root], Is.EqualTo(points[0]));
            Assert.That(tree.InternalArray[LeftChildIndex(tree.Root)], Is.EqualTo(points[1]));
            Assert.That(tree.InternalArray[LeftChildIndex(LeftChildIndex(tree.Root))], Is.EqualTo(points[2]));
            Assert.That(tree.InternalArray[RightChildIndex(LeftChildIndex(tree.Root))], Is.EqualTo(points[3]));
            Assert.That(tree.InternalArray[RightChildIndex(tree.Root)], Is.EqualTo(points[4]));
            Assert.That(tree.InternalArray[LeftChildIndex(RightChildIndex(tree.Root))], Is.EqualTo(points[5]));
        }


        [Test]
        public void FindNearestNeighborTest()
        {
            var dataSize = 10000;
            var testDataSize = 100;
            var range = 1000;

            var treeData = Utilities.GenerateDoubles(dataSize, range);
            var testData = Utilities.GenerateDoubles(testDataSize, range);


            var tree = new KDTree.KDTree<double>(2, double.MinValue, double.MaxValue, Utilities.L2Norm_Squared_Double, treeData);

            for (int i = 0; i < testDataSize; i++)
            {
                var treeNearest = tree.GetNearestNeighbours(testData[i], 1);
                var linearNearest = Utilities.LinearSearch(treeData, testData[i], Utilities.L2Norm_Squared_Double);

                Assert.That(Utilities.L2Norm_Squared_Double(testData[i], linearNearest),
                    Is.EqualTo(Utilities.L2Norm_Squared_Double(testData[i], treeNearest[0])));
            }
        }
    }
}

