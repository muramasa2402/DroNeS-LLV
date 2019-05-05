using System;
using System.Collections.Generic;

namespace Drones.Utils 
{
    public class MinHeap<T> : AbstractBinaryTree<T>
    {
        public MinHeap(Comparison<T> comparer) : base(comparer) { }

        #region Overrides
        public override void ReSort(Comparison<T> comparer)
        {
            var tmp = new List<T>();
            while (!IsEmpty())
            {
                tmp.Add(Remove());
            }
            Clear();
            _comparer = comparer;
            while (tmp.Count > 0)
            {
                Add(tmp[0]);
                tmp.RemoveAt(0);
            }
        }

        public override void ReSort()
        {
            var tmp = new List<T>();
            while (!IsEmpty())
            {
                tmp.Add(Remove());
            }
            Clear();
            while (tmp.Count > 0)
            {
                Add(tmp[0]);
                tmp.RemoveAt(0);
            }
        }

        protected override bool Compare(T entity1, T entity2)
        {
            return _comparer(entity1, entity2) <= 0;
        }
        #endregion
    }
}
