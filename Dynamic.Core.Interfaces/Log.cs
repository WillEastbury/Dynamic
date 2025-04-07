using System;
using System.Threading.Tasks;
using Dynamic.Core.Data.Interfaces;

public class LogEntry<T> where T : IStorable
{
    public LogType LogType { get; set; }
    public T Item { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; } 
    public bool IsHardened { get; set; } = false; // Indicates if the log entry has been hardened.
    
    // Async Task that hardens the log entry (e.g., writing to a persistent store).
    public virtual async Task HardenAsync()
    {
        // Simulate hardening the log entry (e.g., writing to a persistent store).
        // In a real implementation, this would involve more complex logic.
        // for now just wait a random time.
        await Task.Delay(new Random().Next(100, 1000)); // Simulate some delay for hardening.
        IsHardened = true;
    }
}