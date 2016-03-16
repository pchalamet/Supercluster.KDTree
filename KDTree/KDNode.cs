using System;
using System.Text;

namespace KDTree
{
    [Serializable]
    public class KDNode<TKey>
    {
        public KDNode()
        {
        }

        public KDNode(TKey[] point)
        {
            Point = point;
        }

        public TKey[] Point;

        public KDNode<TKey> Left = null;
        public KDNode<TKey> Right = null;

        internal KDNode<TKey> this[int compare]
        {
            get
            {
                if (compare <= 0)
                    return this.Left;
                else
                    return this.Right;
            }
            set
            {
                if (compare <= 0)
                    this.Left = value;
                else
                    this.Right = value;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return (this.Left == null) && (this.Right == null);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var dimension = 0; dimension < Point.Length; dimension++)
            {
                sb.Append(Point[dimension].ToString() + "\t");
            }

            return sb.ToString();
        }
    }
}