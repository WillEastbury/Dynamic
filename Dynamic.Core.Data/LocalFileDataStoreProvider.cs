using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dynamic.Core.Data
{
    public class LocalFileDataStoreProviderConfig
    {
        public string DataStorePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "PrivateData");
    }
    public class LocalFileDataStoreProvider<T> : IDataStoreProvider<T> where T : IStorable
    {
        private LocalFileDataStoreProviderConfig config { get; } 

        public LocalFileDataStoreProvider(LocalFileDataStoreProviderConfig Config)
        { 
            if (!Directory.Exists(Config.DataStorePath))
            {
                Directory.CreateDirectory(Config.DataStorePath);
            }
            this.config = Config;
        }

        public async Task AppendToLogAsync(T item, string LogId)
        {
            var path = Path.Combine(config.DataStorePath, $"{LogId}.json");

            // Open the file stream in append mode so we don't overwrite existing data
            await using var fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);

            // Serialize the object directly to the file stream in UTF-8
            await JsonSerializer.SerializeAsync(fileStream, item);

            // Write a newline after each serialized object for readability
            await fileStream.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine));
        }

        public Task<bool> ChangedAsync(T item)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(T item)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> ReadAllFilteredAsync(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public Task<T?> ReadAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> ReadPagedAsync(int SkipPages, int Pages, int PageSize)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> ReadPagedFilteredAsync(Func<T, bool> filter, int SkipPages, int Pages, int PageSize)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }
    }
}
