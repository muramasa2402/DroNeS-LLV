using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Utilities {

    public class MinHeap<T> {
        private T[] _Queue;
        private int _Size;
        private readonly Func<T, T, int> _Comparer;
        public MinHeap(Func<T, T, int> comparer) 
        {
            Clear();
            _Comparer = comparer;
        }

        public int size
        {
            get
            {
                return _Size;
            }
        }

        public void Add(T element)
        {
            if (_Size == _Queue.Length - 1) { _DoubleSize(); }
            int pos = ++_Size;

            for (; pos > 1 && _Comparer(element, _Queue[pos / 2]) < 0; pos = pos / 2)
            {
                _Queue[pos] = _Queue[pos / 2];
            }

            _Queue[pos] = element;
        }

        public T Remove()
        {
            T head = _Queue[1];

            int n = 1;
            for (; 2 *n < _Size && _Comparer(_Queue[_Size], _Queue[_MinChild(n)]) > 0; n = _MinChild(n))
            {
                _Queue[n] = _Queue[_MinChild(n)];
            }

            _Queue[n] = _Queue[_Size];
            _Queue[_Size--] = default;

            return head;
        }

        public void Clear()
        {
            _Size = 0;
            _Queue = new T[2];
        }

        public bool IsEmpty()
        {
            return _Size == 0;
        }

        public T Get(int index)
        {
            return _Queue[index];
        }

        private void _DoubleSize()
        {
            T[] temp = new T[_Queue.Length * 2];

            for (int i = 0; i <= _Size; i++)
            {
                temp[i] = _Queue[i];
            }

            _Queue = temp;
        }

        private int _MinChild(int n)
        {
            int posChild = 2 * n;
            if (_Comparer(_Queue[posChild], _Queue[posChild + 1]) > 0) { posChild++; }
            return posChild;
        }

    }
}
