using System.Collections.Generic;
using System;
using System.Linq;

namespace Drones.Utils
{
    public class AlertHashSet<T> : HashSet<T>
    {
        public delegate void SetChangeAlert(T item);
#pragma warning disable IDE1006 // Naming Styles
        private event SetChangeAlert _HashSetChanged;
        private event SetChangeAlert _ItemAdded;
        private event SetChangeAlert _ItemRemoved;
#pragma warning restore IDE1006 // Naming Styles

        public event SetChangeAlert SetChanged
        {
            add
            {
                if (_HashSetChanged == null || !_HashSetChanged.GetInvocationList().Contains(value))
                {
                    _HashSetChanged += value;
                }
            }
            remove
            {
                _HashSetChanged -= value;
            }
        }


        public event SetChangeAlert ItemAdded
        {
            add
            {
                if ((_HashSetChanged == null || !_HashSetChanged.GetInvocationList().Contains(value)) &&
                    (_ItemAdded == null || !_ItemAdded.GetInvocationList().Contains(value)))
                {
                    _ItemAdded += value;
                }
            }
            remove
            {
                _ItemAdded -= value;
            }
        }

        public event SetChangeAlert ItemRemoved
        {
            add
            {
                if ((_HashSetChanged == null || !_HashSetChanged.GetInvocationList().Contains(value)) &&
                    (_ItemRemoved == null || !_ItemRemoved.GetInvocationList().Contains(value)))
                {
                    _ItemRemoved += value;
                }
            }
            remove
            {
                _ItemRemoved -= value;
            }
        }

        public Predicate<T> MemberCondition;

        public new bool Add(T item)
        {
            bool a = false;
            if (MemberCondition == null || MemberCondition(item))
            {
                a = base.Add(item);
                if (a) 
                { 
                    _HashSetChanged?.Invoke(item);
                    _ItemAdded?.Invoke(item);
                }
            }
            return a;
        }

        public new bool Remove(T item)
        {
            bool a = base.Remove(item);
            if (a)
            {
                _HashSetChanged?.Invoke(item);
                _ItemRemoved?.Invoke(item);
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
                _HashSetChanged?.Invoke(item);
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
