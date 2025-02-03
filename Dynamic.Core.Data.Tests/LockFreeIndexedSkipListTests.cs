namespace Dynamic.Core.Data.Tests
{
    [TestClass]
    public class LockFreeIndexedSkipListTests
    {
        [TestMethod]
        public void TestInsertAndContains()
        {
            // Arrange
            var skiplist = new LockFreeIndexedSkipList<int>();

            // Act
            bool inserted = skiplist.Insert(5);

            // Assert
            Assert.IsTrue(inserted, "Insert should succeed for a new element.");
            Assert.IsTrue(skiplist.Contains(5), "Skiplist should contain the inserted element.");

            // Act: try inserting a duplicate.
            bool insertedAgain = skiplist.Insert(5);

            // Assert: duplicate insert should fail.
            Assert.IsFalse(insertedAgain, "Insert should return false when trying to insert a duplicate.");
        }

        [TestMethod]
        public void TestRemove()
        {
            // Arrange
            var skiplist = new LockFreeIndexedSkipList<int>();
            skiplist.Insert(10);

            // Act & Assert
            Assert.IsTrue(skiplist.Contains(10), "Skiplist should contain 10 after insertion.");

            bool removed = skiplist.Remove(10);
            Assert.IsTrue(removed, "Remove should succeed when the element exists.");
            Assert.IsFalse(skiplist.Contains(10), "Skiplist should not contain 10 after removal.");

            // Removing a non-existent element should return false.
            bool removedAgain = skiplist.Remove(10);
            Assert.IsFalse(removedAgain, "Remove should return false for an element that does not exist.");
        }

        [TestMethod]
        public void TestGetByIndex()
        {
            // Arrange
            var skiplist = new LockFreeIndexedSkipList<int>();

            // Insert numbers in random order.
            int[] numbers = { 3, 1, 4, 2, 5 };
            foreach (var n in numbers)
            {
                skiplist.Insert(n);
            }
            // Because the skiplist sorts items, the expected order is: 1, 2, 3, 4, 5.

            // Act & Assert: verify each index.
            Assert.AreEqual(1, skiplist.GetByIndex(0), "Index 0 should return the smallest element (1).");
            Assert.AreEqual(2, skiplist.GetByIndex(1), "Index 1 should return the second smallest element (2).");
            Assert.AreEqual(3, skiplist.GetByIndex(2), "Index 2 should return the third smallest element (3).");
            Assert.AreEqual(4, skiplist.GetByIndex(3), "Index 3 should return the fourth smallest element (4).");
            Assert.AreEqual(5, skiplist.GetByIndex(4), "Index 4 should return the fifth smallest element (5).");

            // Act & Assert: accessing an out-of-range index should throw.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => skiplist.GetByIndex(5), "Accessing an invalid index should throw an exception.");
        }

        [TestMethod]
        public void TestEnumeration()
        {
            // Arrange
            var skiplist = new LockFreeIndexedSkipList<int>();

            // Insert some numbers.
            int[] numbers = { 10, 30, 20 };
            foreach (var n in numbers)
            {
                skiplist.Insert(n);
            }
            // The sorted order should be: 10, 20, 30.

            // Act
            List<int> sorted = skiplist.ToList();

            // Assert
            CollectionAssert.AreEqual(new List<int> { 10, 20, 30 }, sorted, "Enumeration should yield elements in sorted order.");
        }

        [TestMethod]
        public void TestFilterMethod()
        {
            // Arrange
            var skiplist = new LockFreeIndexedSkipList<int>();

            // Insert a sequence of numbers.
            int[] numbers = { 1, 2, 3, 4, 5, 6 };
            foreach (var n in numbers)
            {
                skiplist.Insert(n);
            }

            // Act: filter even numbers.
            IEnumerable<int> evens = skiplist.Filter(x => x % 2 == 0);
            List<int> evensList = evens.ToList();

            // Assert
            CollectionAssert.AreEqual(new List<int> { 2, 4, 6 }, evensList, "Filter should correctly yield even numbers.");
        }
    }
}