using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drones.Utils
{

    using Drones.Interface;

    public class SecureSet<T> : ISecureCollectible<T>, IEnumerable<T>
    {
        private event Action<T> CollectionChanged;
        private event Action<T> ItemAddition;
        private event Action<T> ItemRemoval;
        private readonly List<T> _List;
        private readonly HashSet<T> _Set;

        #region Constructors
        public SecureSet()
        {
            _List = new List<T>();
            _Set = new HashSet<T>();
        }

        public SecureSet(IEnumerable<T> collection)
        {
            _List = new List<T>(collection);
            _Set = new HashSet<T>(collection);
        }

        public SecureSet(IEqualityComparer<T> comparer)
        {
            _List = new List<T>();
            _Set = new HashSet<T>(comparer);
        }
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

        public bool Add(T item)
        {

            bool a = false;
            if (MemberCondition == null || MemberCondition(item))
            {
                a = _Set.Add(item);
                if (a) 
                {
                    _List.Add(item);
                    CollectionChanged?.Invoke(item);
                    ItemAddition?.Invoke(item);
                }
            }
            return a;
        }

        public bool Remove(T item)
        {
            bool a = _Set.Remove(item);
            if (a)
            {
                _List.RemoveAt(_List.Count - 1);
                CollectionChanged?.Invoke(item);
                ItemRemoval?.Invoke(item);
            }

            return a;
        }

        public bool Contains(T item)
        {
            return _Set.Contains(item);
        }

        public void Clear()
        {
            _Set.Clear();
            _List.Clear();
        }

        public void Sort()
        {
            _List.Sort();
        }

        public void Sort(IComparer<T> comparer)
        {
            _List.Sort(comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            _List.Sort(comparison);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _List.Sort(index, count, comparer);
        }

        public int CountWhere(Predicate<T> match)
        {
            int i = 0;
            foreach (var item in _Set)
            {
                if (match(item)) { i++; }
            }
            return i;
        }

        public int Count
        {
            get
            {
                return _List.Count;
            }
        }

        public T Peek()
        {
            if (_List.Count > 0)
            {
                return _List[_List.Count - 1];
            }
            return default;
        }

        public T Get()
        {
            if (_List.Count > 0)
            {
                T item = _List[_List.Count - 1];
                _List.RemoveAt(_List.Count - 1);
                _Set.Remove(item);
                return item;
            }
            return default;
        }

        public T this[int i]
        {
            get { return _List[i]; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
