using System;
using System.Collections;

namespace Drones.Utils
{
    public abstract class AbstractBinaryTree<T>
    {
        protected T[] _queue;
        protected Comparison<T> _comparer;
        public int Size { get; private set; }
        public int Count => Size;

        protected AbstractBinaryTree(Comparison<T> comparer)
        {
            Clear();
            _comparer = comparer;
        }

        protected abstract bool Compare(T entity1, T entity2);

        public virtual void Add(T element)
        {
            if (Size == _queue.Length - 1) { DoubleSize(); }
            int pos = ++Size;
            if (_comparer != null)
            {
                while (pos > 1 && Compare(element, _queue[pos / 2]))
                {
                    _queue[pos] = _queue[pos / 2];
                    pos /= 2;
                }
            }

            _queue[pos] = element;

        }

        public virtual T Remove()
        {
            if (IsEmpty()) { throw new NullReferenceException(); }
            T head;
            if (_comparer != null)
            {
                head = _queue[1];
                int n = 1;
                for (; 2 * n < Size && !Compare(_queue[Size], _queue[ChangeNode(n)]); n = ChangeNode(n))
                {
                    _queue[n] = _queue[ChangeNode(n)];
                }
                _queue[n] = _queue[Size];
            }
            else
            {
                head = _queue[Size];
            }

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

        public void ReSort(Comparison<T> comparer = null)
        {
            if (comparer != null)
            {
                _comparer = comparer;
            }

            if (_comparer != null)
            {
                for (int i = Size; i >= 1; i--)
                {
                    Heapify(i);
                }
            }
        }

        private void Swap(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private void Heapify(int i)
        {
            int extreme = i; // Initialize largest as root
            int l = 2 * i; // left = 2*i + 1
            int r = 2 * i + 1; // right = 2*i + 2

            // If left child is larger than root
            if (l <= Size && Compare(_queue[l],_queue[extreme]))
            {
                extreme = l;
            }
                
            // If right child is larger than largest so far
            if (r <= Size && Compare(_queue[r], _queue[extreme]))
            {
                extreme = r;
            }

            // If largest is not root
            if (extreme != i)
            {
                Swap(ref _queue[i], ref _queue[extreme]);
                // Recursively heapify the affected sub-tree
                Heapify(extreme);
            }
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
