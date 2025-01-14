namespace Dynamic.Core.Data;
/// <summary>
/// Interface for a data provider that provides the root of all storage (local disk IO, cloud storage, etc.)
/// </summary>
public interface IDataStoreProvider<T> where T : IStorable
{
    public Task AppendToLogAsync(T item, string LogId);
    public Task CreateAsync(T item);
    public Task<T?> ReadAsync(string Id);
    public Task<Dictionary<string, T>> ReadAllAsync();
    public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize);
    public Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter);
    public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize);
    public Task UpdateAsync(T item);
    public Task DeleteAsync(string Id);
    public Task<bool> ExistsAsync(string Id);
    public Task<bool> ChangedAsync(T item); 
}
