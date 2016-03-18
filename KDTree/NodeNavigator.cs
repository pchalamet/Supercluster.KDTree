namespace Supercluster.KDTree
{
    using static BinaryTreeUtilities;

    public class NodeNavigator<T>
    {
        private T[] array;

        public int Index { get; }

        public NodeNavigator<T> Left
            =>
                LeftChildIndex(this.Index) < this.array.Length - 1
                    ? new NodeNavigator<T>(ref this.array, LeftChildIndex(this.Index))
                    : null;

        public NodeNavigator<T> Right
               =>
                   RightChildIndex(this.Index) < this.array.Length - 1
                       ? new NodeNavigator<T>(ref this.array, RightChildIndex(this.Index))
                       : null;

        public NodeNavigator<T> Parent => this.Index == 0 ? null : new NodeNavigator<T>(ref this.array, ParentIndex(this.Index));

        public T Value => this.array[this.Index];

        public NodeNavigator(ref T[] array, int index = 0)
        {
            this.Index = index;
            this.array = array;
        }
    }
}
