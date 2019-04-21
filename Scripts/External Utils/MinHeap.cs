using System;

namespace Drones.Utils 
{
    public class MinHeap<T> : AbstractBinaryTree<T>
    {
        public MinHeap(Comparison<T> comparer) : base(comparer) { }

        #region Overrides
        public override void ReSort(Comparison<T> comparer)
        {
            var tmp = new MinHeap<T>(comparer);
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
            var tmp = new MinHeap<T>(_comparer);
            while (!IsEmpty())
            {
                tmp.Add(Remove());
            }
            _queue = tmp._queue;
            tmp.Clear();
        }

        protected override bool Compare(T entity1, T entity2)
        {
            return _comparer(entity1, entity2) <= 0;
        }
        #endregion
    }
}
