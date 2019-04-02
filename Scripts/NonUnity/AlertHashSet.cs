using System.Collections.Generic;
using System;
using System.Linq;

namespace Drones.Utils
{
    public class AlertHashSet<T> : HashSet<T>
    {
        public delegate void AlertHandler(T item);
        private event AlertHandler _Alert;
        public event AlertHandler Alert
        {
            add
            {
                if (_Alert == null || !_Alert.GetInvocationList().Contains(value))
                {
                    _Alert += value;
                }
            }
            remove
            {
                _Alert -= value;
            }
        }

        public new bool Add(T item)
        {
            bool a = base.Add(item);
            _Alert?.Invoke(item);

            return a;
        }

        public new bool Remove(T item)
        {
            bool a = base.Remove(item);
            _Alert?.Invoke(item);

            return a;
        }

        public new int RemoveWhere(Predicate<T> match)
        {
            var deleted = new HashSet<T>(this);
            int a = base.RemoveWhere(match);
            deleted.ExceptWith(this);
            foreach (var item in deleted)
            {
                _Alert?.Invoke(item);
            }
            return a;
        }

        public HashSet<T> RetrieveWhere(Predicate<T> match)
        {
            var result = new HashSet<T>(this);
            result.RemoveWhere(x => !match(x));
            return result;
        }

    }
}
