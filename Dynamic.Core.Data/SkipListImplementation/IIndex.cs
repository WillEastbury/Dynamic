using System.Collections.Generic;
public interface IIndex<T> where T : IComparable<T>
{
    public IEnumerable<T> Filter(Func<T, bool> predicate);
    
    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// Note: This traversal is lock‑free and represents a snapshot in time.
    /// </summary>
    IEnumerator<T> GetEnumerator();

    /// <summary>
    /// Inserts a new value into the skiplist.
    /// Returns false if the value already exists.
    /// </summary>
    bool Insert(T value);

    /// <summary>
    /// Removes a value from the skiplist.
    /// Returns true if the removal was successful.
    /// </summary>
    bool Remove(T value);

    /// <summary>
    /// Returns true if the value is present in the skiplist.
    /// </summary>
    bool Contains(T value);

    /// <summary>
    /// Retrieves the value at the specified index (0-based).
    /// </summary>
    T GetByIndex(int index);


}
