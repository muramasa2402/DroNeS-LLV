using System;
using System.Linq;

namespace Drones.Utils
{
    using System.Collections.Generic;
    using Drones.Interface;
    [Serializable]
    public class SecureSortedSet<T0, T1> : ISecureCollectible<T1>
    {
        private event Action<T1> CollectionChanged;
        private event Action<T1> ItemAddition;
        private event Action<T1> ItemRemoval;

        private readonly Map<T0, T1> _Map;
        private readonly MaxHeap<T1> _MaxSorter;
        private readonly MinHeap<T1> _MinSorter;

        #region Constructors
        public SecureSortedSet(Comparison<T1> comparer)
        {
            _Map = new Map<T0, T1>();
            _MaxSorter = new MaxHeap<T1>(comparer);
            _MinSorter = new MinHeap<T1>(comparer);
        }

        public SecureSortedSet()
        {
            _Map = new Map<T0, T1>();
            _MaxSorter = new MaxHeap<T1>(null);
            _MinSorter = new MinHeap<T1>(null);
        }
        #endregion

        #region Events
        public event Action<T1> SetChanged
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

        public event Action<T1> ItemAdded
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

        public event Action<T1> ItemRemoved
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

        public Predicate<T1> MemberCondition { get; set; }

        public bool Add(T0 key, T1 value)
        {

            try
            {
                if (MemberCondition == null || MemberCondition(value))
                {
                    _Map.Add(key, value);
                    _MaxSorter.Add(value);
                    _MinSorter.Add(value);
                    CollectionChanged?.Invoke(value);
                    ItemAddition?.Invoke(value);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Remove(T1 value)
        {
            try
            {
                _Map.Remove(value);
                CollectionChanged?.Invoke(value);
                ItemAddition?.Invoke(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Remove(T0 key)
        {
            try
            {
                var item = _Map.Forward[key];
                _Map.Remove(key);
                CollectionChanged?.Invoke(item);
                ItemAddition?.Invoke(item);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Dictionary<T0, T1>.ValueCollection Values => _Map.Forward.Values;

        public Dictionary<T0, T1>.KeyCollection Keys => _Map.Forward.Keys;

        public T1 FindValue(T0 key)
        {
            try
            {
                return _Map.Forward[key];
            }
            catch (Exception)
            {
                return default;
            }
        }

        public int Count => _Map.Count();

        public void Clear() => _Map.Clear();

        public bool Contains(T0 key) => _Map.Contains(key);

        public bool Contains(T1 val) => _Map.Contains(val);

        public int CountWhere(Predicate<T1> match)
        {
            int i = 0;
            foreach (var item in _Map.Forward.Values)
            {
                if (match(item)) { i++; }
            }
            return i;
        }

        public T1 GetMax(bool sort)
        {
            try
            {
                if (Count > 0)
                {
                    T1 output;
                    if (sort) _MaxSorter.ReSort();
                    do
                    {
                        output = _MaxSorter.Remove();
                    } while (!Remove(output));

                    return output;
                }
                return default;
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }

        public T1 GetMin(bool sort)
        {
            try
            {
                if (Count > 0)
                {
                    T1 output;
                    if (sort) _MinSorter.ReSort();
                    do
                    {
                        output = _MinSorter.Remove();
                    } while (!Remove(output));

                    return output;
                }
                return default;
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }

        public T1 PeekMax(bool sort)
        {
            try
            {
                if (Count > 0)
                {
                    T1 output;
                    if (sort) _MaxSorter.ReSort();
                    do
                    {
                        output = _MaxSorter.Remove();
                    } while (!_Map.Contains(output));

                    return output;
                }
                return default;
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }

        public T1 PeekMin(bool sort)
        {
            try
            {
                if (Count > 0)
                {
                    T1 output;
                    if (sort) _MinSorter.ReSort();
                    do
                    {
                        output = _MinSorter.Remove();
                    } while (!_Map.Contains(output));

                    return output;
                }
                return default;
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }

        public T1 Find(Predicate<T1> match)
        {
            foreach (var item in _Map.Forward.Values)
            {
                if (match(item))
                {
                    return item;
                }
            }
            return default;
        }

        public T1 this[T0 key] => _Map.Forward[key];

        public T0 this[T1 key] => _Map.Reverse[key];

        public void ReSort()
        {
            _MaxSorter.ReSort();
            _MinSorter.ReSort();
        }

        public void ReSort(Comparison<T1> comparer)
        {
            _MaxSorter.ReSort(comparer);
            _MinSorter.ReSort(comparer);
        }
    }
}
