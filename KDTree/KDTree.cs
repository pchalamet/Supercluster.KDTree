using System;

namespace KDTree
{
    using System.Linq;
    using static BinaryTreeUtilities;

    [Serializable]
    public class KDTree<TKey> where TKey : IComparable<TKey>
    {

        private readonly TKey minvalue;
        private readonly TKey maxvalue;
        public readonly TKey[][] InternalArray;
        private readonly Func<TKey[], TKey[], TKey> Metric;

        public KDTree(int dimensions, TKey minValue, TKey maxValue, Func<TKey[], TKey[], TKey> metric, TKey[][] points)
        {
            this.dimensions = dimensions;
            var elementCount = (int)Math.Pow(2, (int)(Math.Log(points.Length) / Math.Log(2)) + 1);
            this.InternalArray = Enumerable.Repeat(default(TKey[]), elementCount).ToArray();
            this.minvalue = minValue;
            this.maxvalue = maxValue;
            this.Metric = metric;

            this.GrowTree(0, 0, points);
            this.Root = 0;
        }

        private readonly int dimensions;

        public int Root = -1;

        /// <summary>
        /// Grows a KD tree recursively
        /// </summary>
        /// <param name="currentNode">The current node in the recursion.</param>
        /// <param name="previousNode">The node in the previous level of the recursion. This is als the parent of the cirrent node.</param>
        /// <param name="points">The set of points remaining to be added to the kd-tree</param>
        /// <param name="dim">The current splitting dimension of the kd-tree. This is the depth mod K, where K is the dimensionality of the data set and depth is the depth of the tree.</param>
        public void GrowTree(int index, int dim, TKey[][] points)
        {
            // TODO: we need to some how set the count of the KD tree

            // See wikipedia for a good explanation kd-tree construction.
            // https://en.wikipedia.org/wiki/K-d_tree

            // sort the points along the current dimension
            var sortedPoints = points.OrderBy(p => p[dim]).ToArray();

            // get the point which has the median value of the current dimension.
            var medianPoint = sortedPoints[points.Length / 2];
            var medianPointIdx = sortedPoints.Length / 2;

            // The point with the median value all the current dimension now becomes the value of the current tree node
            // The previous node becomes the parents of the current node.
            this.InternalArray[index] = medianPoint;



            // We now split the sorted points into 2 groups
            // 1st group: points before the median
            var leftPoints = new TKey[medianPointIdx][];
            Array.Copy(sortedPoints, leftPoints, leftPoints.Length);

            // 2nd group: Points after the median
            var rightPoints = new TKey[sortedPoints.Length - (medianPointIdx + 1)][];
            Array.Copy(sortedPoints, medianPointIdx + 1, rightPoints, 0, rightPoints.Length);

            // We new recurse, passing the left and right arrays for arguments.
            // The current node's left and right values become the "roots" for
            // each recursion call. We also forward cycle to the next dimension.

            var nextDim = (dim + 1) % this.dimensions; // select next dimension

            // We only need to recurse if the point array contains more than one point
            // If the array has no points then the node stay a null value

            if (leftPoints.Length <= 1)
            {
                if (leftPoints.Length == 1)
                {
                    this.InternalArray[LeftChildIndex(index)] = leftPoints[0];
                }
            }
            else
            {
                this.GrowTree(LeftChildIndex(index), nextDim, leftPoints);
            }

            // Do the same for the right points
            if (rightPoints.Length <= 1)
            {
                if (rightPoints.Length == 1)
                {
                    this.InternalArray[RightChildIndex(index)] = rightPoints[0];
                }
            }
            else
            {
                this.GrowTree(RightChildIndex(index), nextDim, rightPoints);
            }
        }

        public TKey[][] GetNearestNeighbours(TKey[] point, int count)
        {

            /*
                TODO: We should check if there are any nodes to avoid errors
            */

            var nearestNeighbours = new BoundedPriorityList<TKey[], TKey>(count);

            var rect = HyperRect<TKey>.Infinite(dimensions, this.maxvalue, this.minvalue);

            AddNearestNeighbours(this.Root, point, rect, 0, nearestNeighbours, this.maxvalue);

            return nearestNeighbours.ToArray();
        }

        /*
         * 1. Search for the target
         * 
         *   1.1 Start by splitting the specified hyper rect
         *       on the specified node's point along the current
         *       dimension so that we end up with 2 sub hyper rects
         *       (current dimension = depth % dimensions)
         *   
         *	 1.2 Check what sub rectangle the the target point resides in
         *	     under the current dimension
         *	     
         *   1.3 Set that rect to the nearer rect and also the corresponding 
         *       child node to the nearest rect and node and the other rect 
         *       and child node to the further rect and child node (for use later)
         *       
         *   1.4 Travel into the nearer rect and node by calling function
         *       recursively with nearer rect and node and incrementing 
         *       the depth
         * 
         * 2. Add leaf to list of nearest neighbours
         * 
         * 3. Walk back up tree and at each level:
         * 
         *    3.1 Add node to nearest neighbours if
         *        we haven't filled our nearest neighbour
         *        list yet or if it has a distance to target less
         *        than any of the distances in our current nearest 
         *        neighbours.
         *        
         *    3.2 If there is any point in the further rectangle that is closer to
         *        the target than our furtherest nearest neighbour then travel into
         *        that rect and node
         * 
         *  That's it, when it finally finishes traversing the branches 
         *  it needs to we'll have our list!
         */

        private void AddNearestNeighbours(
            int nodeIndex,
            TKey[] target,
            HyperRect<TKey> rect,
            int depth,
            BoundedPriorityList<TKey[], TKey> nearestNeighbours,
            TKey maxSearchRadiusSquared)
        {
            if (this.InternalArray.Length <= nodeIndex || nodeIndex < 0 || this.InternalArray[nodeIndex] == null)
            {
                return;
            }

            // Work out the current dimension
            int dimension = depth % this.dimensions;

            // Split our hyper-rect into 2 sub rects along the current 
            // node's point on the current dimension
            var leftRect = rect.Clone();
            leftRect.MaxPoint[dimension] = this.InternalArray[nodeIndex][dimension];

            var rightRect = rect.Clone();
            rightRect.MinPoint[dimension] = this.InternalArray[nodeIndex][dimension];

            // Which side does the target reside in?
            int compare = target[dimension].CompareTo(this.InternalArray[nodeIndex][dimension]);

            var nearerRect = compare <= 0 ? leftRect : rightRect;
            var furtherRect = compare <= 0 ? rightRect : leftRect;

            var nearerNode = compare <= 0 ? LeftChildIndex(nodeIndex) : RightChildIndex(nodeIndex);
            var furtherNode = compare <= 0 ? RightChildIndex(nodeIndex) : LeftChildIndex(nodeIndex);

            // Let's walk down into the nearer branch
            if (nearerNode != null)
            {
                this.AddNearestNeighbours(
                    nearerNode,
                    target,
                    nearerRect,
                    depth + 1,
                    nearestNeighbours,
                    maxSearchRadiusSquared);
            }

            TKey distanceSquaredToTarget;

            // Walk down into the further branch but only if our capacity hasn't been reached 
            // OR if there's a region in the further rect that's closer to the target than our
            // current furtherest nearest neighbour
            TKey[] closestPointInFurtherRect = furtherRect.GetClosestPoint(target);
            distanceSquaredToTarget = this.Metric(closestPointInFurtherRect, target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                if (nearestNeighbours.IsFull)
                {
                    if (distanceSquaredToTarget.CompareTo(nearestNeighbours.MaxPriority) < 0)
                        this.AddNearestNeighbours(
                            furtherNode,
                            target,
                            furtherRect,
                            depth + 1,
                            nearestNeighbours,
                            maxSearchRadiusSquared);
                }
                else
                {
                    this.AddNearestNeighbours(
                        furtherNode,
                        target,
                        furtherRect,
                        depth + 1,
                        nearestNeighbours,
                        maxSearchRadiusSquared);
                }
            }

            // Try to add the current node to our nearest neighbours list
            distanceSquaredToTarget = this.Metric(this.InternalArray[nodeIndex], target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
                nearestNeighbours.Add(this.InternalArray[nodeIndex], distanceSquaredToTarget);
        }

        /*
        public KDNode<TKey>[] RadialSearch(TKey[] center, TKey radius, int count)
        {
            var nearestNeighbours = new NearestNeighbourList<KDNode<TKey>, TKey>(count, this.minvalue);

            AddNearestNeighbours(
                Root,
                center,
                HyperRect<TKey>.Infinite(dimensions, typeMath),
                0,
                nearestNeighbours,
                typeMath.Multiply(radius, radius));

            count = nearestNeighbours.Count;

            var neighbourArray = new KDNode<TKey>[count];

            for (var index = 0; index < count; index++)
                neighbourArray[count - index - 1] = nearestNeighbours.RemoveFurtherest();

            return neighbourArray;
        }*/

        public int Count { get; private set; }

    }
}