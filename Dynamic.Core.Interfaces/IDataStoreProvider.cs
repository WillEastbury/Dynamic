using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.Core.Data.Interfaces;
/// <summary>
/// Interface for a data provider that provides the root of all storage (local disk IO, cloud storage, etc.)
/// </summary>
public interface IDataStoreProvider<T> where T : IStorable
{
    public Task AppendToLogAsync(LogType log, T item, bool WaitForHarden, string userId);
    public Task CreateAsync(T item, bool waitForHarden, string userId);
    public Task<T?> ReadAsync(string Id, bool waitForHarden);
    public Task<Dictionary<string, T>> ReadAllAsync();
    public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize);
    public Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter);
    public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize);
    public Task UpdateAsync(T item, bool WaitForHarden, string userId);
    public Task DeleteAsync(string Id, bool WaitForHarden, string userId);
    public Task<bool> ExistsAsync(string Id, bool WaitForHarden);
    public Task<bool> ChangedAsync(T item, bool WaitForHarden); 
}
