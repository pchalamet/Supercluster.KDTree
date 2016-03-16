using System.Collections.Generic;


namespace KDTree
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    public class BoundedPriorityList<TElement, TPriority> : IEnumerable<TElement> where TPriority : IComparable<TPriority>
    {
        public TElement MaxElement => this.elementList[this.elementList.Count - 1];
        public TPriority MaxPriority => this.priorityList[this.priorityList.Count - 1];

        public TElement MinElement => this.elementList[0];

        public TPriority MinPriority => this.priorityList[0];

        public int Capacity { get; private set; }

        public bool IsFull => this.Count == this.Capacity;

        public int Count => this.priorityList.Count;

        public BoundedPriorityList(int capacity)
        {

            this.Capacity = capacity;
            this.priorityList = new List<TPriority>(capacity);
            this.elementList = new List<TElement>(capacity);
        }

        private readonly List<TElement> elementList;
        private readonly List<TPriority> priorityList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TElement item, TPriority priority)
        {
            if (this.Count >= this.Capacity)
            {
                if (this.priorityList[this.priorityList.Count - 1].CompareTo(priority) < 0)
                {
                    return;
                }

                var index = this.priorityList.BinarySearch(priority);
                index = index >= 0 ? index : ~index;

                this.priorityList.Insert(index, priority);
                this.elementList.Insert(index, item);

                this.priorityList.RemoveAt(this.priorityList.Count - 1);
                this.elementList.RemoveAt(this.elementList.Count - 1);
            }
            else
            {
                var index = this.priorityList.BinarySearch(priority);
                index = index >= 0 ? index : ~index;

                this.priorityList.Insert(index, priority);
                this.elementList.Insert(index, item);
            }
        }

        public TElement this[int index] => this.elementList[index];

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elementList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
