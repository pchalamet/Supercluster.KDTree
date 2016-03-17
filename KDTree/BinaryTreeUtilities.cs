namespace Supercluster.KDTree
{
    using System.Runtime.CompilerServices;

    public static class BinaryTreeUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RightChildIndex(int index)
        {
            return (2 * index) + 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeftChildIndex(int index)
        {
            return (2 * index) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParentIndex(int index)
        {
            return (index - 1) / 2;
        }

    }
}
