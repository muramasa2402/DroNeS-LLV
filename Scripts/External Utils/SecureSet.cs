using System.Collections.Generic;
using System;
using System.Linq;

namespace Drones.Utils
{
    using System.Collections;
    using Drones.Interface;
    using UnityEngine;

    public class SecureSet<T> : ISecureCollectible<T>, IEnumerable<T>
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

        private readonly Stack<T> _Stack;
        private readonly HashSet<T> _Set;

        public SecureSet()
        {
            _Stack = new Stack<T>();
            _Set = new HashSet<T>();
        }

        public SecureSet(IEnumerable<T> collection)
        {
            _Stack = new Stack<T>(collection);
            _Set = new HashSet<T>(collection);
        }

        public SecureSet(IEqualityComparer<T> comparer)
        {
            _Stack = new Stack<T>();
            _Set = new HashSet<T>(comparer);
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

        public bool Add(T item)
        {

            bool a = false;
            if (MemberCondition == null || MemberCondition(item))
            {
                a = _Set.Add(item);
                if (a) 
                {
                    _Stack.Push(item);
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
                _Stack.Pop();
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
            _Stack.Clear();
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
                return _Stack.Count;
            }
        }

        public T Peek()
        {
            if (_Stack.Count > 0)
            {
                return _Stack.Peek();
            }
            return default;
        }

        public T Get()
        {
            if (_Stack.Count > 0)
            {
                T item = _Stack.Pop();
                _Set.Remove(item);
                return item;
            }
            return default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _Set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
