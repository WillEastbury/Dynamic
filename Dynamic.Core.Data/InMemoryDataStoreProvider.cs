using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Dynamic.Core.Data.Interfaces;

namespace Dynamic.Core.Data;

    /// <summary>
    /// InMemoryDataStoreProvider is a simple in-memory data store provider that implements the IDataStoreProvider interface.
    /// /// It uses a ConcurrentDictionary to store items and provides methods for creating, reading, updating, and deleting items.
    /// /// It also supports filtering and pagination of items.
    /// </summary>
    /// <typeparam name="T">The type of items stored in the data store. Must implement IStorable.</typeparam>
    /// <remarks>
    /// This class is thread-safe. But it is not suitable for production use as it does not persist data to disk.
    /// </remarks>
    /// 
    public class InMemoryDataStoreProvider<T> : IDataStoreProvider<T>, IEnumerable<T> where T : IStorable
    {
        private readonly ConcurrentDictionary<string, T> _store = new (); // A dictionary to store items by their Id.
    
        private readonly List<LogEntry<T>> _logs = new();     // A list to store logs of items created.

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _idIndexes = new(); // A dictionary that holds an index for each public property of T.

        private readonly Dictionary<string, PropertyInfo> _indexedProperties = new Dictionary<string, PropertyInfo>();  // We also store the corresponding PropertyInfo for each property so we can read the value.
        private const string CompositeKeyDelimiter = "||"; // A delimiter used in composite keys (used for non-Id properties).

        public InMemoryDataStoreProvider()
        {
            // Reflect over the public properties of T and create the appropriate dictionaries for each one.
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // Optionally, you might filter here to only index a subset.
                _indexedProperties[prop.Name] = prop;
                _idIndexes[prop.Name] = new ConcurrentDictionary<string, object>();
            }
        }

        public async Task AppendToLogAsync(LogType log, T item, bool WaitForHarden = false, string userId = "System")
        {
            LogEntry<T> logEntry = new LogEntry<T> { LogType = log, Item = item, Timestamp = DateTime.UtcNow, UserId = userId };
            _logs.Add(logEntry);
            if(WaitForHarden)
            {
                await HardenCheck(logEntry);
            }
        }

        private static async Task HardenCheck(LogEntry<T> logEntry)
        {
            while (!logEntry.IsHardened)
            {
                // Wait for the item to be hardened
                await Task.Delay(100); // Wait for 100ms before checking again
            }
        }
        private async Task HardenTraverse(string Id)
        {
            // Find the correct log entry for the last CUD operation on the item.
            LogEntry<T>? logEntry = _logs.LastOrDefault(x => x.Item.Id == Id && (x.LogType == LogType.Create || x.LogType == LogType.Update || x.LogType == LogType.Delete));
            while (!logEntry.IsHardened && logEntry != null)
            {
                // Wait for the item to be hardened
                await Task.Delay(100); // Wait for 100ms before checking again
            }
}

        public async Task CreateAsync(T item, bool waitForHarden = false, string userId = "System")
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (! string.IsNullOrEmpty(item.Id)) throw new ArgumentException("Item Id MUST be null or empty.", nameof(item));

            item.Id = Guid.NewGuid().ToString(); // Generate a new Id for the item.
            _store[item.Id] = item; // Add the item to the store.
            await AppendToLogAsync(LogType.Create, item, waitForHarden, userId); // Log the item creation.

            // Index the item by its properties.
            foreach (var prop in _indexedProperties)
            {
                var value = prop.Value.GetValue(item)?.ToString() ?? string.Empty;
                _idIndexes[prop.Key][value] = item.Id;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _store.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    public async Task<T?> ReadAsync(string Id, bool waitForHarden)
    {
        if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
        if (waitForHarden)
        {
            await HardenTraverse(Id);
        }
        if (_store.TryGetValue(Id, out T? item))
        {
            return item;
        }
        return default; // Return null if the item is not found.
    }

    public Task<Dictionary<string, T>> ReadAllAsync()
    {
        return Task.FromResult(_store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize)
    {
        if (SkipPages < 0) throw new ArgumentOutOfRangeException(nameof(SkipPages), "SkipPages cannot be negative.");
        if (Pages < 0) throw new ArgumentOutOfRangeException(nameof(Pages), "Pages cannot be negative.");
        if (PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(PageSize), "PageSize must be greater than zero.");

        return Task.FromResult(_store.Skip(SkipPages * PageSize).Take(Pages * PageSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        
    }

    public async Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter)
    {
        // ToDo: Update to include filters by indexed properties.
        if (filter == null) throw new ArgumentNullException(nameof(filter));
        var outy = await ReadAllAsync();
        return outy.Where(kvp => filter(kvp.Value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize)
    {
        // Todo: Update to include filters by indexed properties.
        if (filter == null) throw new ArgumentNullException(nameof(filter));
        if (SkipPages < 0) throw new ArgumentOutOfRangeException(nameof(SkipPages), "SkipPages cannot be negative.");
        if (Pages < 0) throw new ArgumentOutOfRangeException(nameof(Pages), "Pages cannot be negative.");
        if (PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(PageSize), "PageSize must be greater than zero.");

        return Task.FromResult(_store.Where(kvp => filter(kvp.Value)).Skip(SkipPages * PageSize).Take(Pages * PageSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

    }

    public async Task UpdateAsync(T item, bool WaitForHarden, string userId)
    {
        // Update the item in the store.
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (string.IsNullOrEmpty(item.Id)) throw new ArgumentException("Item Id cannot be null or empty.", nameof(item));

        if (_store.TryGetValue(item.Id, out T? existingItem))
        {
            _store[item.Id] = item; // Update the item in the store.
            await AppendToLogAsync(LogType.Update, item, WaitForHarden, userId); // Log the item update.

            // Re-index the item by its properties.
            foreach (var prop in _indexedProperties)
            {
                var value = prop.Value.GetValue(item)?.ToString() ?? string.Empty;
                _idIndexes[prop.Key][value] = item.Id;
            }
        }
        else
        {
            throw new KeyNotFoundException($"Item with Id {item.Id} not found.");
        }
    }
    public async Task DeleteAsync(string Id, bool WaitForHarden = false, string userId = "System")
    {
        if (Id == null || Id == "") throw new ArgumentNullException(nameof(Id));
        if (WaitForHarden)
        {
            await HardenTraverse(Id); // Wait for the item to be hardened.
        }
        await AppendToLogAsync(LogType.Delete, _store[Id], WaitForHarden, userId); // Log the item deletion.
        if (_store.TryRemove(Id, out T? item))
        {
            // Remove the item from the store then kill the indexes
            foreach (var prop in _indexedProperties)
            {
                var value = prop.Value.GetValue(item)?.ToString() ?? string.Empty;
                _idIndexes[prop.Key].TryRemove(value, out _); // Remove the index entry for the property.
            }
        }
        else
        {
            throw new KeyNotFoundException($"Item with Id {Id} not found.");
        }
    }

    public Task<bool> ExistsAsync(string Id, bool WaitForHarden)
    {
        return Task.FromResult(_store.ContainsKey(Id));
    }

    public Task<bool> ChangedAsync(T item, bool WaitForHarden)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (string.IsNullOrEmpty(item.Id)) throw new ArgumentException("Item Id cannot be null or empty.", nameof(item));

        if (_store.TryGetValue(item.Id, out T? existingItem))
        {
            return Task.FromResult(!JsonSerializer.Serialize(existingItem).Equals(JsonSerializer.Serialize(item)));
        }
        return Task.FromResult(false); // Return false if the item is not found.
    }
}
