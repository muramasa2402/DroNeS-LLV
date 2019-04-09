using System.Collections.Generic;
using System.Linq;

namespace Drones.Utils
{
    using System;
    using Interface;
    public class SecureSortedSet<T> : SortedSet<T>, ISecureCollectible<T>
    {
        private event AlertHandler<T> CollectionChanged;
        private event AlertHandler<T> ItemAddition;
        private event AlertHandler<T> ItemRemoval;

        public event AlertHandler<T> SetChanged
        {
            add
            {
                if (CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value))
                {
                    CollectionChanged += value;
                }
            }
            remove
            {
                CollectionChanged -= value;
            }
        }

        public event AlertHandler<T> ItemAdded
        {
            add
            {
                if ((CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value)) &&
                    (ItemAddition == null || !ItemAddition.GetInvocationList().Contains(value)))
                {
                    ItemAddition += value;
                }
            }
            remove
            {
                ItemAddition -= value;
            }
        }

        public event AlertHandler<T> ItemRemoved
        {
            add
            {
                if ((CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value)) &&
                    (ItemRemoval == null || !ItemRemoval.GetInvocationList().Contains(value)))
                {
                    ItemRemoval += value;
                }
            }
            remove
            {
                ItemRemoval -= value;
            }
        }

        public Predicate<T> MemberCondition { get; set; }

        public SecureSortedSet() { }

        public SecureSortedSet(IEnumerable<T> collection) : base(collection)
        { }

        public SecureSortedSet(IComparer<T> comparer) : base(comparer)
        { }

        public new bool Add(T item)
        {
            bool a = false;
            if (MemberCondition == null || MemberCondition(item))
            {
                a = base.Add(item);
                if (a)
                {
                    CollectionChanged?.Invoke(item);
                    ItemAddition?.Invoke(item);
                }
            }
            return a;
        }

        public new bool Remove(T item)
        {
            bool a = base.Remove(item);
            if (a)
            {
                CollectionChanged?.Invoke(item);
                ItemRemoval?.Invoke(item);
            }

            return a;
        }

        public new int RemoveWhere(Predicate<T> match)
        {
            var deleted = new SortedSet<T>(this);
            int a = base.RemoveWhere(match);
            deleted.ExceptWith(this);
            foreach (var item in deleted)
            {
                CollectionChanged?.Invoke(item);
                ItemRemoval?.Invoke(item);
            }
            return a;
        }

        public SortedSet<T> RetrieveWhere(Predicate<T> match)
        {
            var result = new SortedSet<T>(this);
            result.RemoveWhere(x => !match(x));
            return result;
        }

        public int CountWhere(Predicate<T> match)
        {
            int i = 0;
            foreach (var item in this)
            {
                if (match(item)) { i++; }
            }
            return i;
        }


    }

}