using System.Text.Json;
namespace Dynamic.Core.Data;

public class InMemoryDataStoreProvider<T> : IDataStoreProvider<T> where T : IStorable
{
    private readonly Dictionary<string, List<T>> _logs = new Dictionary<string, List<T>>();
    private readonly Dictionary<string, T> _store = new Dictionary<string, T>();

    public Task AppendToLogAsync(T item, string LogId)
    {
        if (!_logs.ContainsKey(LogId))
        {
            _logs[LogId] = new List<T>();
        }
        _logs[LogId].Add(item);
        return Task.CompletedTask;
    }

    public Task CreateAsync(T item)
    {
        item.Id = Guid.NewGuid().ToString();
        _store[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<T?> ReadAsync(string Id)
    {
        if (!_logs.ContainsKey(Id))
        {
            return Task.FromResult<T?>(_store[Id]);
        }
        return Task.FromResult(default(T));
    }

    public Task<Dictionary<string, T>> ReadAllAsync()
    {
        return Task.FromResult(_store);
    }

    public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize)
    {
        var skippedItems = _store.Values.Skip(SkipPages * PageSize);
        var pagedItems = skippedItems.Take(Pages * PageSize).ToDictionary(x => x.Id, x => x);
        return Task.FromResult(pagedItems);
    }

    public Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter)
    {
        var filteredItems = _store.Values.Where(filter).ToDictionary(x => x.Id, x => x);
       return Task.FromResult(filteredItems);
    }

    public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize)
    {
        var filteredItems = _store.Values.Where(filter);
        var skippedItems = filteredItems.Skip(SkipPages * PageSize);
        var pagedItems = skippedItems.Take(Pages * PageSize).ToDictionary(x => x.Id, x => x);
        return Task.FromResult(pagedItems);
    }

    public Task UpdateAsync(T item)
    {
        if (_store.ContainsKey(item.Id))
        {
            _store[item.Id] = item;
            return Task.FromResult(item);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string Id)
    {
        if (_store.ContainsKey(Id))
        {
            _store.Remove(Id);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string Id)
    {
        return Task.FromResult(_store.ContainsKey(Id));
    }

    public Task<bool> ChangedAsync(T item)
    {
        if (_store.ContainsKey(item.Id))
        {
            var storedItem = _store[item.Id];

            // serialize both of them, hash them and compare them 
            string storedItemJson = JsonSerializer.Serialize<T>(storedItem);
            string itemJson = JsonSerializer.Serialize<T>(item);

            if (storedItemJson != itemJson)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}
