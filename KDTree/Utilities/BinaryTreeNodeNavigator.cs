// <copyright file="BinaryTreeNodeNavigator.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree.Utilities
{
    using System;

    using static BinaryTreeNavigation;

    /// <summary>
    /// Allows one to navigate a binary tree stored in an <see cref="Array"/> using familiar
    /// tree navigation concepts.
    /// </summary>
    /// <typeparam name="T">The type of the individual nodes.</typeparam>
    public class BinaryTreeNodeNavigator<T>
    {
        /// <summary>
        /// A reference to the array in which the binary tree is stored in.
        /// </summary>
        private T[] array;

        /// <summary>
        /// The index in the array that the current node resides in.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The left child of the current node.
        /// </summary>
        public BinaryTreeNodeNavigator<T> Left
            =>
                LeftChildIndex(this.Index) < this.array.Length - 1
                    ? new BinaryTreeNodeNavigator<T>(this.array, LeftChildIndex(this.Index))
                    : null;

        /// <summary>
        /// The right child of the current node.
        /// </summary>
        public BinaryTreeNodeNavigator<T> Right
               =>
                   RightChildIndex(this.Index) < this.array.Length - 1
                       ? new BinaryTreeNodeNavigator<T>(this.array, RightChildIndex(this.Index))
                       : null;

        /// <summary>
        /// The parent of the current node.
        /// </summary>
        public BinaryTreeNodeNavigator<T> Parent => this.Index == 0 ? null : new BinaryTreeNodeNavigator<T>(this.array, ParentIndex(this.Index));

        /// <summary>
        /// The value of the current node.
        /// </summary>
        public T Value => this.array[this.Index];

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryTreeNodeNavigator{T}"/> class.
        /// </summary>
        /// <param name="array">The array backing the binary tree.</param>
        /// <param name="index">The index of the node of interest in the array. If not given, the node navigator start at the 0 index (the root of the tree).</param>
        public BinaryTreeNodeNavigator(T[] array, int index = 0)
        {
            this.Index = index;
            this.array = array;
        }
    }
}
