using System;
using Drones.Utils;
using NUnit.Framework;

namespace Tests.External
{
    [TestFixture]
    public class BinaryTreeTest
    {
        private int IntComparer(int a, int b) {
            return a - b;
        }

        private readonly int[] elements = { 4, 2, 3, 5, 7, 1 };

        [Test]
        public void AddingToMinHeapAndMaxHeapIncreasesSize()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);

            Assert.AreEqual(0, minHeap.Size);
            Assert.AreEqual(0, maxHeap.Size);

            int size = 0;
            foreach (int element in elements)
            {
                minHeap.Add(element);
                maxHeap.Add(element);
                size++;
                Assert.AreEqual(size, minHeap.Size);
                Assert.AreEqual(size, maxHeap.Size);
            }
        }

        [Test]
        public void RemovingFromMinHeapAndMaxHeapDecreasesSize()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);

            int size = elements.Length;
            foreach (int element in elements)
            {
                minHeap.Add(element);
                maxHeap.Add(element);
            }
            while (minHeap.Size > 0 && maxHeap.Size > 0)
            {
                minHeap.Remove();
                maxHeap.Remove();
                size--;
                Assert.AreEqual(size, minHeap.Size);
                Assert.AreEqual(size, maxHeap.Size);
            }

        }

        [Test]
        public void TopElementInHeapsCanBeRead()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            int elem = 1;
            minHeap.Add(elem);
            Assert.AreEqual(elem, minHeap.Peek());
            maxHeap.Add(elem);
            Assert.AreEqual(elem, maxHeap.Peek());
        }

        [Test]
        public void MinimumValueWillAlwaysBeAtTheHeadInMinHeapWhenAdding()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            int minimum = elements[0];
            foreach(int element in elements)
            {
                if (minimum > element) { minimum = element; }
                minHeap.Add(element);
                Assert.AreEqual(minimum, minHeap.Peek());
            }
        }

        [Test]
        public void MaximumValueWillAlwaysBeAtTheHeadInMaxHeapWhenAdding()
        {
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            int maximum = elements[0];
            foreach (int element in elements)
            {
                if (maximum < element) { maximum = element; }
                maxHeap.Add(element);
                Assert.AreEqual(maximum, maxHeap.Peek());
            }
        }

        [Test]
        public void MinimumValueWillAlwaysBeAtTheHeadInMinHeapWhenRemoving()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            int[] expected = { 1, 2, 3, 4, 5, 7 };
            foreach (int element in elements)
            {
                minHeap.Add(element);
            }
            int i = 0;
            while (minHeap.Size > 0)
            {
                Assert.AreEqual(expected[i++], minHeap.Remove());
            }
        }

        [Test]
        public void MaximumValueWillAlwaysBeAtTheHeadInMaxHeapWhenRemoving()
        {
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            int[] expected = { 7, 5, 4, 3, 2, 1 };
            foreach (int element in elements)
            {
                maxHeap.Add(element);
            }
            int i = 0;
            while (maxHeap.Size > 0)
            {
                Assert.AreEqual(expected[i++], maxHeap.Remove());
            }
        }

        [Test]
        public void AttemptToPeekOrRemoveFromEmptyMaxHeapWillThrowException()
        {
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            int item;
            try
            {
                item = maxHeap.Peek();
                Assert.Fail("Exception Expected");
            }
            catch (NullReferenceException) { }
            try
            {
                item = maxHeap.Remove();
                Assert.Fail("Exception Expected");
            }
            catch (NullReferenceException) { }
        }

        [Test]
        public void AttemptToPeekOrRemoveFromEmptyMinHeapWillThrowException()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            int item;
            try
            {
                item = minHeap.Peek();
                Assert.Fail("Exception Expected");
            }
            catch (NullReferenceException) { }
            try
            {
                item = minHeap.Remove();
                Assert.Fail("Exception Expected");
            }
            catch (NullReferenceException) { }
        }

        [Test]
        public void ClearingHeapsSetsSizeToZero()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            foreach (int element in elements)
            {
                maxHeap.Add(element);
                minHeap.Add(element);
            }
            maxHeap.Clear();
            minHeap.Clear();
            Assert.AreEqual(0, maxHeap.Size);
            Assert.AreEqual(0, minHeap.Size);

        }

        [Test]
        public void FILOTest()
        {
            MinHeap<int> minHeap = new MinHeap<int>(null);
            MaxHeap<int> maxHeap = new MaxHeap<int>(null);
            foreach (int element in elements)
            {
                minHeap.Add(element);
                maxHeap.Add(element);
            }
            int i = elements.Length - 1;
            while (!minHeap.IsEmpty())
            {
                Assert.AreEqual(elements[i--], minHeap.Remove());
            }
            i = elements.Length - 1;
            while (!maxHeap.IsEmpty())
            {
                Assert.AreEqual(elements[i--], maxHeap.Remove());
            }
        }
        internal class Vector2
        {
            public Vector2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
            public float x { get; set; }
            public float y { get; set; }
            public float Magnitude => (float)Math.Sqrt(x * x + y * y);
            public override string ToString() => "(" + x + ", " + y + ")";
        }
        [Test]
        public void ReSortWithOldComparerTest()
        {
            MinHeap<Vector2> minHeap = new MinHeap<Vector2>((Vector2 a, Vector2 b) => (a.Magnitude <= b.Magnitude) ? -1 : 1);
            MaxHeap<Vector2> maxHeap = new MaxHeap<Vector2>((Vector2 a, Vector2 b) => (a.Magnitude <= b.Magnitude) ? -1 : 1);
            Vector2[] vs = { new Vector2(0.1f, 0.1f), new Vector2(1, 1), new Vector2(2, 3), new Vector2(1, 2) };
            foreach (var vec in vs)
            {
                minHeap.Add(vec);
                maxHeap.Add(vec);
            }
            vs[0].x = 5;
            vs[0].y = 5;
            minHeap.ReSort();
            maxHeap.ReSort();
            int[] r = { 1, 3, 2, 0 };
            int i = 0;
            while (!minHeap.IsEmpty())
            {
                Assert.AreSame(vs[r[i++]], minHeap.Remove());
            }
            i = r.Length - 1;
            while (!maxHeap.IsEmpty())
            {
                Assert.AreSame(vs[r[i--]], maxHeap.Remove());
            }

        }

        [Test]
        public void ReSortWithNewComparerTest()
        {
            MinHeap<int> minHeap = new MinHeap<int>(IntComparer);
            MaxHeap<int> maxHeap = new MaxHeap<int>(IntComparer);
            foreach (int element in elements)
            {
                minHeap.Add(element);
                maxHeap.Add(element);
            }
            minHeap.ReSort((int a, int b) => a * 10 % 17 - b * 10 % 17);
            maxHeap.ReSort((int a, int b) => a * 10 % 17 - b * 10 % 17);
            int[] minresult = { 7, 2, 4, 1, 3, 5 };
            int[] maxresult = { 5, 3, 1, 4, 2, 7 };
            int i = 0;
            while (!minHeap.IsEmpty())
            {
                Assert.AreEqual(minresult[i++], minHeap.Remove());
            }
            i = 0;
            while (!maxHeap.IsEmpty())
            {
                Assert.AreEqual(maxresult[i++], maxHeap.Remove());
            }

        }

    }
}
