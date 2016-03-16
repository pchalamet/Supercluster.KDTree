namespace KDTree
{
    using System;
    public struct HyperRect<T> where T : IComparable<T>
    {
        private T[] minPoint;
        public T[] MinPoint
        {
            get
            {
                return minPoint;
            }
            set
            {
                minPoint = new T[value.Length];
                value.CopyTo(minPoint, 0);
            }
        }

        private T[] maxPoint;
        public T[] MaxPoint
        {
            get
            {
                return maxPoint;
            }
            set
            {
                maxPoint = new T[value.Length];
                value.CopyTo(maxPoint, 0);
            }
        }

        public static HyperRect<T> Infinite(int dimensions, T PositiveInfinity, T NegativeInfinity)
        {
            var rect = new HyperRect<T>();

            rect.MinPoint = new T[dimensions];
            rect.MaxPoint = new T[dimensions];

            for (var dimension = 0; dimension < dimensions; dimension++)
            {
                rect.MinPoint[dimension] = NegativeInfinity;
                rect.MaxPoint[dimension] = PositiveInfinity;
            }

            return rect;
        }

        public T[] GetClosestPoint(T[] toPoint)
        {
            T[] closest = new T[toPoint.Length];

            for (var dimension = 0; dimension < toPoint.Length; dimension++)
            {
                if (minPoint[dimension].CompareTo(toPoint[dimension]) > 0)
                {
                    closest[dimension] = minPoint[dimension];
                }
                else if (maxPoint[dimension].CompareTo(toPoint[dimension]) < 0)
                {
                    closest[dimension] = maxPoint[dimension];
                }
                else
                    // Point is within rectangle, at least on this dimension
                    closest[dimension] = toPoint[dimension];
            }

            return closest;
        }

        public HyperRect<T> Clone()
        {
            var rect = new HyperRect<T>();
            rect.MinPoint = MinPoint;
            rect.MaxPoint = MaxPoint;
            return rect;
        }
    }
}
