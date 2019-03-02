using System;

namespace Utilities
{
    public class MaxHeap<T> : BinaryTree<T>
    {
        public MaxHeap(Func<T, T, int> comparer) : base(comparer) { }

        #region Overrides
        protected override bool Compare(T entity1, T entity2)
        {
            return _comparer(entity1, entity2) >= 0;
        }
        #endregion

    }

}
