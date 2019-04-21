using System;
using System.Collections;

namespace Drones.Utils
{
    public class MaxHeap<T> : AbstractBinaryTree<T>
    {
        public MaxHeap(Comparison<T> comparer) : base(comparer) { }

        #region Overrides
        public override void ReSort(Comparison<T> comparer)
        {
            var tmp = new MaxHeap<T>(comparer);
            while (!IsEmpty())
            {
                tmp.Add(Remove());
            }
            _comparer = comparer;
            _queue = tmp._queue;
            tmp.Clear();
        }

        public override void ReSort()
        {
            var tmp = new MaxHeap<T>(_comparer);
            while (!IsEmpty())
            {
                tmp.Add(Remove());
            }
            _queue = tmp._queue;
            tmp.Clear();
        }

        protected override bool Compare(T entity1, T entity2)
        {
            return _comparer(entity1, entity2) >= 0;
        }
        #endregion

    }

}
