
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Utils;

    public class ListHeaders : MonoBehaviour
    {
        private AbstractListWindow _Window;
        public AbstractListWindow Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = (AbstractListWindow)AbstractWindow.GetWindow(transform);
                }
                return _Window;
            }
        }

        public ListTupleContainer TupleContainer
        {
            get
            {
                return Window.TupleContainer;
            }
        }

        private Button[] _Headers;
        public Button[] Headers
        {
            get
            {
                if (_Headers == null)
                {
                    _Headers = GetComponentsInChildren<Button>();
                }
                return _Headers;
            }
        }
        private SortOrder[] _Order;
        public SortOrder[] Order
        {
            get
            {
                if (_Order == null)
                {
                    _Order = new SortOrder[Headers.Length];
                    for (int i = 0; i < _Order.Length; i++)
                    {
                        _Order[i] = SortOrder.Ascending;
                    }
                }
                return _Order;
            }
        }

        private void Start()
        {
            foreach (Button header in Headers)
            {
                header.onClick.AddListener(delegate
                {
                    Sort(header.transform.GetSiblingIndex());
                });
            }
        }

        // The indexing will be consistent due to horizontal layout
        // index : data index in the tuple
        private void Sort(int index)
        {
            AbstractBinaryTree<ListTuple> heap;
            ListTuple[] tuples = TupleContainer.GetComponentsInChildren<ListTuple>();
            int Comparer(ListTuple a, ListTuple b)
            {
                return string.Compare(a.Data[index].text, b.Data[index].text);
            }

            if (Order[index] == SortOrder.Ascending)
            {
                heap = new MinHeap<ListTuple>(Comparer);
                Order[index] = SortOrder.Descending;
            }
            else
            {
                heap = new MaxHeap<ListTuple>(Comparer);
                Order[index] = SortOrder.Ascending;
            }

            for (int i = 0; i < tuples.Length; i++)
            {
                heap.Add(tuples[i]);
            }

            int n = 0;
            while (!heap.IsEmpty())
            {
                heap.Remove().transform.SetSiblingIndex(n++);
            }

        }
    }

}

