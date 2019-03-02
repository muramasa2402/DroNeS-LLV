using System;

namespace Utilities 
{
    public class MinHeap<T> : BinaryTree<T>
    {
        public MinHeap(Func<T, T, int> comparer) : base(comparer) { }

        #region Overrides
        protected override bool Compare(T entity1, T entity2)
        {
            return _comparer(entity1, entity2) <= 0;
        }
        #endregion
    }
}
