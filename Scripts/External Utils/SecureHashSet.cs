using System.Collections.Generic;
using System;
using System.Linq;

namespace Drones.Utils
{
    using Drones.Interface;
    public class SecureHashSet<T> : HashSet<T>, ISecureCollectible<T>
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

        public SecureHashSet()
        { }

        public SecureHashSet(IEnumerable<T> collection) : base(collection)
        { }

        public SecureHashSet(IEqualityComparer<T> comparer) : base(comparer)
        { }

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
            var deleted = new HashSet<T>(this);
            int a = base.RemoveWhere(match);
            deleted.ExceptWith(this);
            foreach (var item in deleted)
            {
                CollectionChanged?.Invoke(item);
                ItemRemoval?.Invoke(item);
            }
            return a;
        }

        public HashSet<T> RetrieveWhere(Predicate<T> match)
        {
            var result = new HashSet<T>(this);
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
