using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json; 

namespace dynamic.data.tests;
{
    public class TestItem : IStorable
    {
        public string id { get; set; }
        public string FilterTest { get; set; }
        public string Name { get; set; }
    }

    [TestClass]
    public class DataStoreProviderTests
    {
        private IDataStoreProvider<TestItem> _dataStoreProvider;

        [TestInitialize]
        public void SetUp()
        {
            _dataStoreProvider = new MockDataStoreProvider();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCreateItemWithValidId()
        {
            // Arrange
            var item = new TestItem { Name = "Test Item" };

            // Act
            var createdItem = await _dataStoreProvider.CreateAsync(item);

            // Assert
            Assert.IsNotNull(createdItem.id);
            Assert.AreEqual(item.Name, createdItem.Name);
        }

        [TestMethod]
        public async Task ReadAsync_ShouldReturnItemById()
        {
            // Arrange
            var item = new TestItem { Name = "Test Item" };
            var createdItem = await _dataStoreProvider.CreateAsync(item);

            // Act
            var fetchedItem = await _dataStoreProvider.ReadAsync(createdItem.id);

            // Assert
            Assert.IsNotNull(fetchedItem);
            Assert.AreEqual(createdItem.id, fetchedItem.id);
            Assert.AreEqual(createdItem.Name, fetchedItem.Name);
        }

        [TestMethod]
        public async Task ReadAsync_ShouldReturnNullForNonExistentId()
        {
            // Act
            var fetchedItem = await _dataStoreProvider.ReadAsync("nonExistentId");

            // Assert
            Assert.IsNull(fetchedItem);
        }

        [TestMethod]
        public async Task ReadAllAsync_ShouldReturnAllItems()
        {
            // Arrange
            await _dataStoreProvider.CreateAsync(new TestItem { Name = "Item 1" });
            await _dataStoreProvider.CreateAsync(new TestItem { Name = "Item 2" });

            // Act
            var items = await _dataStoreProvider.ReadAllAsync();

            // Assert
            Assert.AreEqual(2, items.Count);
        }

        [TestMethod]
        public async Task ReadPagedAsync_ShouldReturnPagedItems()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                await _dataStoreProvider.CreateAsync(new TestItem { Name = $"Item {i}" });
            }

            // Act
            var pagedItems = await _dataStoreProvider.ReadPagedAsync(0, 1, 5);

            // Assert
            Assert.AreEqual(5, pagedItems.Count);
        }

        [TestMethod]
        public async Task ReadAllFilteredAsync_ShouldReturnFilteredItems()
        {
            // Arrange
            await _dataStoreProvider.CreateAsync(new TestItem { Name = "Item A" });
            await _dataStoreProvider.CreateAsync(new TestItem { Name = "Item B" });
            await _dataStoreProvider.CreateAsync(new TestItem { Name = "Item C" });

            // Act
            var filteredItems = await _dataStoreProvider.ReadAllFilteredAsync(x => x.Name.StartsWith("Item A"));

            // Assert
            Assert.AreEqual(1, filteredItems.Count);
            Assert.AreEqual("Item A", filteredItems.Values.First().Name);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldUpdateItem()
        {
            // Arrange
            var item = await _dataStoreProvider.CreateAsync(new TestItem { Name = "Old Item" });

            // Act
            item.Name = "Updated Item";
            var updatedItem = await _dataStoreProvider.UpdateAsync(item);

            // Assert
            Assert.AreEqual("Updated Item", updatedIt
