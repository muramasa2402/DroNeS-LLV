using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drones.Utils
{

    using Drones.Interface;

    public class SecureSet<T> : HashSet<T>, ISecureCollectible<T>
    {
        private event Action<T> CollectionChanged;
        private event Action<T> ItemAddition;
        private event Action<T> ItemRemoval;

        #region Constructors
        public SecureSet(){ }

        public SecureSet(IEnumerable<T> collection) : base(collection) { }

        public SecureSet(IEqualityComparer<T> comparer) : base(comparer) { }
        #endregion

        #region Events
        public event Action<T> SetChanged
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

        public event Action<T> ItemAdded
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

        public event Action<T> ItemRemoved
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
        #endregion

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

        public int CountWhere(Predicate<T> match)
        {
            int i = 0;
            foreach (var item in this)
            {
                if (match(item)) { i++; }
            }
            return i;
        }

        // Gets a "random" value from the set
        public T Get()
        {
            if (Count > 0)
            {
                T item = this[Count - 1];
                if (Remove(item))
                {
                    return item;
                }
            }
            return default;
        }

        public T Find(Predicate<T> match)
        {
            foreach (T item in this)
            {
                if (match(item))
                {
                    return item;
                }
            }
            return default;
        }

        public List<T> ToList() => new List<T>(this);

        public List<T1> ToList<T1>() => ToList().Cast<T1>().ToList();

        public T this[int i] => this.ElementAt(i);
    }
}
