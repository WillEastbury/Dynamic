namespace Dynamic.Core.Data;
/// <summary>
/// Interface for a data provider that provides the root of all storage (local disk IO, cloud storage, etc.)
/// </summary>
/// <summary>
/// Interface for a class / object that has a unique ID that can be stored and retrieved as a key
/// </summary>
public interface IStorable
{
    public string Id { get; set; }
}