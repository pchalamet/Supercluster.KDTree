namespace KDTree
{
    using System.Runtime.CompilerServices;

    internal static class BinaryTreeUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int RightChildIndex(int index)
        {
            return 2 * index + 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LeftChildIndex(int index)
        {
            return 2 * index + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ParentIndex(int index)
        {
            return (index - 1) / 2;
        }

    }
}
