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

        private int[] elements = { 4, 2, 3, 5, 7, 1 };

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

    }
}
