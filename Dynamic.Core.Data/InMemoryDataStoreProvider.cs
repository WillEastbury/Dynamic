using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Linq; 
namespace Dynamic.Core.Data;

    /// <summary>
    /// An in‑memory data store provider that maintains one skiplist index per public property of T.
    /// The provider implements IEnumerable&lt;T&gt; so that clients may use LINQ queries against it,
    /// and it transparently uses the underlying skiplist indexes.
    /// </summary>
    public class InMemoryDataStoreProvider<T> : IDataStoreProvider<T>, IEnumerable<T> where T : IStorable
    {
        // The primary data store for fast lookup by Id.
        private readonly Dictionary<string, T> _store = new Dictionary<string, T>();

        // Log storage (unchanged from your original code).
        private readonly Dictionary<string, List<T>> _logs = new Dictionary<string, List<T>>();

        // A dictionary that holds an index (skiplist) for each public property of T.
        private readonly Dictionary<string, LockFreeIndexedSkipList<string>> _idIndexes = new Dictionary<string, LockFreeIndexedSkipList<string>>();
        // We also store the corresponding PropertyInfo for each property so we can read the value.
        private readonly Dictionary<string, PropertyInfo> _indexedProperties = new Dictionary<string, PropertyInfo>();

        // A delimiter used in composite keys (used for non-Id properties).
        private const string CompositeKeyDelimiter = "||";

        public InMemoryDataStoreProvider()
        {
            // Reflect over the public properties of T.
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // Optionally, you might filter here to only index a subset.
                _indexedProperties[prop.Name] = prop;
                _idIndexes[prop.Name] = new LockFreeIndexedSkipList<string>();
            }
        }

        public Task AppendToLogAsync(T item, string LogId)
        {
            if (!_logs.ContainsKey(LogId))
            {
                _logs[LogId] = new List<T>();
            }
            _logs[LogId].Add(item);
            return Task.CompletedTask;
        }

        public Task<T> CreateAsync(T item)
        {
            // Generate and assign a new Id.
            item.Id = Guid.NewGuid().ToString();
            _store[item.Id] = item;
            // Update all indexes with the new item.
            foreach (var kvp in _indexedProperties)
            {
                string propName = kvp.Key;
                string compositeKey = CreateCompositeKey(propName, item);
                _idIndexes[propName].Insert(compositeKey);
            }
            return Task.FromResult(item);
        }

        public Task<T?> ReadAsync(string Id)
        {
            _store.TryGetValue(Id, out T? item);
            return Task.FromResult(item);
        }

        public Task<Dictionary<string, T>> ReadAllAsync() => Task.FromResult(_store);

        public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize)
        {
            var pagedItems = _store.Values
                                   .Skip(SkipPages * PageSize)
                                   .Take(Pages * PageSize)
                                   .ToDictionary(x => x.Id, x => x);
            return Task.FromResult(pagedItems);
        }

        public Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter)
        {
            // Here we use a default index (for example, the "Id" index if available; otherwise, the first index).
            string defaultIndex = _indexedProperties.ContainsKey("Id") ? "Id" : _indexedProperties.Keys.First();
            var result = new Dictionary<string, T>();
            foreach (var compositeKey in _idIndexes[defaultIndex])
            {
                string id = ExtractIdFromCompositeKey(defaultIndex, compositeKey);
                if (_store.TryGetValue(id, out var item) && filter(item))
                {
                    result[id] = item;
                }
            }
            return Task.FromResult(result);
        }

        public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize)
        {
            string defaultIndex = _indexedProperties.ContainsKey("Id") ? "Id" : _indexedProperties.Keys.First();
            var filtered = _idIndexes[defaultIndex]
                .Select(compositeKey => ExtractIdFromCompositeKey(defaultIndex, compositeKey))
                .Where(id => _store.ContainsKey(id) && filter(_store[id]))
                .Skip(SkipPages * PageSize)
                .Take(Pages * PageSize)
                .Select(id => _store[id])
                .ToDictionary(item => item.Id, item => item);
            return Task.FromResult(filtered);
        }

        public Task UpdateAsync(T item)
        {
            if (_store.ContainsKey(item.Id))
            {
                // Remove the item’s old index entries...
                RemoveIndexesForItem(item);
                // Update the store.
                _store[item.Id] = item;
                // ...and re-add the indexes for the updated item.
                AddIndexesForItem(item);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string Id)
        {
            if (_store.TryGetValue(Id, out var item))
            {
                RemoveIndexesForItem(item);
                _store.Remove(Id);
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string Id) => Task.FromResult(_store.ContainsKey(Id));

        public Task<bool> ChangedAsync(T item)
        {
            if (_store.TryGetValue(item.Id, out var storedItem))
            {
                string storedJson = JsonSerializer.Serialize(storedItem);
                string itemJson = JsonSerializer.Serialize(item);
                return Task.FromResult(storedJson != itemJson);
            }
            return Task.FromResult(false);
        }

        // Create a composite key for a given property.
        // For the "Id" property the key is simply the Id.
        // For any other property, we combine its value and the Id.
        private string CreateCompositeKey(string propertyName, T item)
        {
            if (!_indexedProperties.TryGetValue(propertyName, out var propInfo))
                throw new Exception($"Property {propertyName} is not indexed.");

            if (propertyName == "Id")
                return item.Id;

            var value = propInfo.GetValue(item)?.ToString() ?? string.Empty;
            return $"{value}{CompositeKeyDelimiter}{item.Id}";
        }

        // Extract the item Id from a composite key.
        private string ExtractIdFromCompositeKey(string propertyName, string compositeKey)
        {
            if (propertyName == "Id")
                return compositeKey;

            var parts = compositeKey.Split(new string[] { CompositeKeyDelimiter }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1] : compositeKey;
        }

        private void RemoveIndexesForItem(T item)
        {
            foreach (var propName in _indexedProperties.Keys)
            {
                string compositeKey = CreateCompositeKey(propName, item);
                _idIndexes[propName].Remove(compositeKey);
            }
        }

        private void AddIndexesForItem(T item)
        {
            foreach (var propName in _indexedProperties.Keys)
            {
                string compositeKey = CreateCompositeKey(propName, item);
                _idIndexes[propName].Insert(compositeKey);
            }
        }

        // The provider implements IEnumerable<T> so that clients can query it via LINQ.
        // Here we use a default index (the "Id" index if available, otherwise the first defined index)
        // to yield items in sorted order.
        public IEnumerator<T> GetEnumerator()
        {
            string defaultIndex = _indexedProperties.ContainsKey("Id") ? "Id" : _indexedProperties.Keys.First();
            foreach (var compositeKey in _idIndexes[defaultIndex])
            {
                string id = ExtractIdFromCompositeKey(defaultIndex, compositeKey);
                if (_store.TryGetValue(id, out var item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Allows querying on a specific indexed property.
        /// For example, to query on the "Name" property, pass in "Name" along with a predicate on the property value.
        /// </summary>
        public IEnumerable<T> QueryByIndex(string propertyName, Func<string, bool> predicate)
        {
            if (!_idIndexes.ContainsKey(propertyName))
                throw new ArgumentException($"Index for property {propertyName} does not exist.");

            foreach (var compositeKey in _idIndexes[propertyName])
            {
                string propertyValue;
                string id;
                if (propertyName == "Id")
                {
                    propertyValue = compositeKey;
                    id = compositeKey;
                }
                else
                {
                    var parts = compositeKey.Split(new string[] { CompositeKeyDelimiter }, StringSplitOptions.None);
                    propertyValue = parts[0];
                    id = parts.Length > 1 ? parts[1] : compositeKey;
                }
                if (predicate(propertyValue) && _store.ContainsKey(id))
                {
                    yield return _store[id];
                }
            }
        }

    Task IDataStoreProvider<T>.CreateAsync(T item)
    {
        return CreateAsync(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        string defaultIndex = _indexedProperties.ContainsKey("Id") ? "Id" : _indexedProperties.Keys.First();
            foreach (var compositeKey in _idIndexes[defaultIndex])
            {
                string id = ExtractIdFromCompositeKey(defaultIndex, compositeKey);
                if (_store.TryGetValue(id, out var item))
                {
                    yield return item;
                }
            }
    }

}