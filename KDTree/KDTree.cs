// <copyright file="KDTree.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace KDTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Supercluster.KDTree;

    using static Supercluster.KDTree.Utilities.BinaryTreeNavigation;

    [Serializable]
    public class KDTree<TDimension>
        where TDimension : IComparable<TDimension>
    {
        /// <summary>
        /// The numbers of dimensions that the tree has.
        /// </summary>
        public int Dimensions { get; private set; }

        /// <summary>
        /// The metric function used to calculate distance between points.
        /// </summary>
        public Func<TDimension[], TDimension[], double> Metric { get; set; }

        private TDimension MinValue { get; }

        private TDimension MaxValue { get; }

        /// <summary>
        /// The array in which the binary tree is stored. Enumerating this array is a level-order traversal of the tree.
        /// </summary>
        public readonly TDimension[][] InternalArray;

        public KDTree(int dimensions, IEnumerable<TDimension[]> points, Func<TDimension[], TDimension[], double> metric, TDimension searchWindowMinValue = default(TDimension), TDimension searchWindowMaxValue = default(TDimension))
        {

            if (searchWindowMinValue.Equals(default(TDimension)))
            {
                var type = typeof(TDimension);
                this.MinValue = (TDimension)type.GetField("MinValue").GetValue(type);
            }
            else
            {
                this.MinValue = searchWindowMinValue;
            }

            if (searchWindowMaxValue.Equals(default(TDimension)))
            {
                var type = typeof(TDimension);
                this.MaxValue = (TDimension)type.GetField("MaxValue").GetValue(type);
            }
            else
            {
                this.MaxValue = searchWindowMaxValue;
            }

            var pointsArray = points.ToArray();
            var elementCount = (int)Math.Pow(2, (int)(Math.Log(pointsArray.Length) / Math.Log(2)) + 1);
            this.Dimensions = dimensions;
            this.InternalArray = Enumerable.Repeat(default(TDimension[]), elementCount).ToArray();
            this.Metric = metric;
            this.Count = pointsArray.Length;
            this.GenerateTree(0, 0, pointsArray);
        }

        /// <summary>
        /// Finds the nearest neighbors in the <see cref="KDTree{TDimension}"/> of the given <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point whose neighbors we search for.</param>
        /// <param name="neighbors">The number of neighboors to look for.</param>
        /// <returns>The</returns>
        public TDimension[][] NearestNeighbors(TDimension[] point, int neighbors)
        {
            var nearestNeighborList = new BoundedPriorityList<TDimension[], double>(neighbors);
            var rect = HyperRect<TDimension>.Infinite(this.Dimensions, this.MaxValue, this.MinValue);
            this.SearchForNearestNeighbors(0, point, rect, 0, nearestNeighborList, double.MaxValue);
            return nearestNeighborList.ToArray();
        }

        /// <summary>
        /// Searches for the closest points in a hyper-sphere around the given center.
        /// </summary>
        /// <param name="center">The center of the hyper-sphere</param>
        /// <param name="radius">The radius of the hyper-sphere</param>
        /// <param name="neighboors">The number of neighbors to return.</param>
        /// <returns>The specified number of closest points in the hyper-sphere</returns>
        public TDimension[][] RadialSearch(TDimension[] center, double radius, int neighboors = -1)
        {
            var nearestNeighbors = new BoundedPriorityList<TDimension[], double>(neighboors == -1 ? this.Count : neighboors);

            this.SearchForNearestNeighbors(
                0,
                center,
                HyperRect<TDimension>.Infinite(this.Dimensions, this.MaxValue, this.MinValue),
                0,
                nearestNeighbors,
                radius * radius);

            var nn = nearestNeighbors.ToList();
            nn.TrimExcess();
            return nn.ToArray();
        }

        /// <summary>
        /// Grows a KD tree recursively via media splitting. We find the median by doing a full sort.
        /// </summary>
        /// <param name="index">The array index for the current node.</param>
        /// <param name="dim">The current splitting dimension.</param>
        /// <param name="points">The set of points remaining to be added to the kd-tree</param>
        private void GenerateTree(int index, int dim, TDimension[][] points)
        {
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
            var leftPoints = new TDimension[medianPointIdx][];
            Array.Copy(sortedPoints, leftPoints, leftPoints.Length);

            // 2nd group: Points after the median
            var rightPoints = new TDimension[sortedPoints.Length - (medianPointIdx + 1)][];
            Array.Copy(sortedPoints, medianPointIdx + 1, rightPoints, 0, rightPoints.Length);

            // We new recurse, passing the left and right arrays for arguments.
            // The current node's left and right values become the "roots" for
            // each recursion call. We also forward cycle to the next dimension.
            var nextDim = (dim + 1) % this.Dimensions; // select next dimension

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
                this.GenerateTree(LeftChildIndex(index), nextDim, leftPoints);
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
                this.GenerateTree(RightChildIndex(index), nextDim, rightPoints);
            }
        }

        /*
        * 1. Search for the target``
        * 
        *   1.1 Start by splitting the specified hyper rect
        *       on the specified node's point along the current
        *       dimension so that we end up with 2 sub hyper rects
        *       (current dimension = depth % dimensions)
        *   
        *   1.2 Check what sub rectangle the the target point resides in
        *       under the current dimension
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

        private void SearchForNearestNeighbors(
            int nodeIndex,
            TDimension[] target,
            HyperRect<TDimension> rect,
            int depth,
            BoundedPriorityList<TDimension[], double> nearestNeighbors,
            double maxSearchRadiusSquared)
        {
            if (this.InternalArray.Length <= nodeIndex || nodeIndex < 0 || this.InternalArray[nodeIndex] == null)
            {
                return;
            }

            // Work out the current dimension
            int dimension = depth % this.Dimensions;

            // Split our hyper-rect into 2 sub rects along the current 
            // node's point on the current dimension
            var leftRect = rect.Clone();
            leftRect.MaxPoint[dimension] = this.InternalArray[nodeIndex][dimension];

            var rightRect = rect.Clone();
            rightRect.MinPoint[dimension] = this.InternalArray[nodeIndex][dimension];

            // Which side does the target reside in?
            var compare = target[dimension].CompareTo(this.InternalArray[nodeIndex][dimension]);

            var nearerRect = compare <= 0 ? leftRect : rightRect;
            var furtherRect = compare <= 0 ? rightRect : leftRect;

            var nearerNode = compare <= 0 ? LeftChildIndex(nodeIndex) : RightChildIndex(nodeIndex);
            var furtherNode = compare <= 0 ? RightChildIndex(nodeIndex) : LeftChildIndex(nodeIndex);

            // Let's walk down into the nearer branch
            this.SearchForNearestNeighbors(
                nearerNode,
                target,
                nearerRect,
                depth + 1,
                nearestNeighbors,
                maxSearchRadiusSquared);

            // Walk down into the further branch but only if our capacity hasn't been reached 
            // OR if there's a region in the further rect that's closer to the target than our
            // current furtherest nearest neighbor
            var closestPointInFurtherRect = furtherRect.GetClosestPoint(target);
            double distanceSquaredToTarget = this.Metric(closestPointInFurtherRect, target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                if (nearestNeighbors.IsFull)
                {
                    if (distanceSquaredToTarget.CompareTo(nearestNeighbors.MaxPriority) < 0)
                    {
                        this.SearchForNearestNeighbors(
                            furtherNode,
                            target,
                            furtherRect,
                            depth + 1,
                            nearestNeighbors,
                            maxSearchRadiusSquared);
                    }
                }
                else
                {
                    this.SearchForNearestNeighbors(
                        furtherNode,
                        target,
                        furtherRect,
                        depth + 1,
                        nearestNeighbors,
                        maxSearchRadiusSquared);
                }
            }

            // Try to add the current node to our nearest neighbors list
            distanceSquaredToTarget = this.Metric(this.InternalArray[nodeIndex], target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                nearestNeighbors.Add(this.InternalArray[nodeIndex], distanceSquaredToTarget);
            }
        }

        /// <summary>
        /// The number of points in the KDTree
        /// </summary>
        public int Count { get; }
    }
}