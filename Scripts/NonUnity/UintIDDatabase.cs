using System;
using System.Collections.Generic;

namespace Drones.Utils
{
    public class UintIDDatabase : HashSet<uint>
    {
        Queue<uint> _Removed = new Queue<uint>();
        public new uint Add(uint i)
        {
            if (_Removed.Count != 0)
            {
                i = _Removed.Dequeue();
            }

            while (!base.Add(i))
            {
                i++;
            }

            return i;
        }

        public new bool Remove(uint item)
        {
            if (base.Remove(item))
            {
                _Removed.Enqueue(item);
                return true;
            }
            return false;
        }

        public new int RemoveWhere(Predicate<uint> match)
        {
            var deleted = new HashSet<uint>(this);
            int a = base.RemoveWhere(match);
            deleted.ExceptWith(this);
            foreach (var item in deleted)
            {
                _Removed.Enqueue(item);
            }
            return a;
        }
    }
}
