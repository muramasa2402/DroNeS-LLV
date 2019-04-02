using System;

namespace Drones.Utils
{
    public abstract class AbstractBinaryTree<T>
    {
        protected T[] _queue;
        public delegate int CompareFunction(T item1, T item2);
        protected readonly CompareFunction _comparer;

        public int Size { get; private set; }

        protected AbstractBinaryTree(CompareFunction comparer)
        {
            Clear();
            _comparer = comparer;
        }

        protected abstract bool Compare(T entity1, T entity2);

        public virtual void Add(T element)
        {
            if (Size == _queue.Length - 1) { DoubleSize(); }
            int pos = ++Size;

            while (pos > 1 && Compare(element, _queue[pos / 2]))
            {
                _queue[pos] = _queue[pos / 2];
                pos = pos / 2;
            }

            _queue[pos] = element;
        }

        public virtual T Remove()
        {
            if (IsEmpty()) { throw new NullReferenceException(); }
            T head = _queue[1];

            int n = 1;
            for (; 2 * n < Size && !Compare(_queue[Size], _queue[ChangeNode(n)]); n = ChangeNode(n))
            {
                _queue[n] = _queue[ChangeNode(n)];
            }

            _queue[n] = _queue[Size];
            _queue[Size--] = default;

            return head;
        }

        public void Clear()
        {
            Size = 0;
            _queue = new T[2];
        }

        public bool IsEmpty()
        {
            return Size == 0;
        }

        public T Peek() 
        {
            if (IsEmpty()) throw new NullReferenceException(); return _queue[1];
        }

        protected void DoubleSize()
        {
            T[] temp = new T[_queue.Length * 2];

            for (int i = 0; i <= Size; i++)
            {
                temp[i] = _queue[i];
            }

            _queue = temp;
        }

        protected int ChangeNode(int n)
        {
            int posChild = 2 * n;
            if (!Compare(_queue[posChild], _queue[posChild + 1])) { posChild++; }
            return posChild;
        }
    }
}
