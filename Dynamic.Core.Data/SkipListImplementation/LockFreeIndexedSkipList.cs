using System.Collections;

/// <summary>
/// A simplified (demonstration-only) lock-free indexed skiplist.
/// Implements IEnumerable<T> so that it can be used with LINQ, and IIndex<T> for indexed access.
/// </summary>
public class LockFreeIndexedSkipList<T> : IEnumerable<T>, IIndex<T> where T : IComparable<T>
{
    private readonly SkipListNode<T> head;
    private const int MaxLevel = 32;
    private readonly Random random = new Random();

    public LockFreeIndexedSkipList()
    {
        // The head is a sentinel node with the maximum level.
        head = new SkipListNode<T>(MaxLevel, default(T));
    }

    /// <summary>
    /// Filters the skiplist using the provided predicate.
    /// </summary>
    public IEnumerable<T> Filter(Func<T, bool> predicate)
    {
        foreach (T item in this)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Randomly selects a level for a new node.
    /// </summary>
    private int RandomLevel()
    {
        int level = 1;
        while (level < MaxLevel && random.NextDouble() < 0.5)
            level++;
        return level;
    }

    /// <summary>
    /// Internal find method used by multiple operations.
    /// It also “helps” by removing logically deleted nodes.
    /// </summary>
    private bool Find(T value, SkipListNode<T>[] preds, SkipListNode<T>[] succs)
    {
    Restart:
        SkipListNode<T> pred = head;
        for (int level = MaxLevel - 1; level >= 0; level--)
        {
            int stamp;
            SkipListNode<T> curr = pred.Next[level].Get(out stamp);
            while (true)
            {
                if (curr != null)
                {
                    int currStamp;
                    SkipListNode<T> succ = curr.Next[level].Get(out currStamp);
                    // Help remove logically deleted nodes.
                    if (Volatile.Read(ref curr.Marked) == 1)
                    {
                        int predStamp;
                        SkipListNode<T> predNext = pred.Next[level].Get(out predStamp);
                        if (!pred.Next[level].CompareAndSet(curr, succ, predStamp, predStamp + 1))
                        {
                            goto Restart;
                        }
                        curr = succ;
                        continue;
                    }
                    if (curr.Value.CompareTo(value) < 0)
                    {
                        pred = curr;
                        curr = curr.Next[level].Get(out stamp);
                        continue;
                    }
                }
                preds[level] = pred;
                succs[level] = curr;
                break;
            }
        }
        return (succs[0] != null && succs[0].Value.CompareTo(value) == 0);
    }

    /// <summary>
    /// Inserts a value into the skiplist.
    /// Returns false if the value already exists.
    /// </summary>
    public bool Insert(T value)
    {
        int topLevel = RandomLevel();
        var preds = new SkipListNode<T>[MaxLevel];
        var succs = new SkipListNode<T>[MaxLevel];

        while (true)
        {
            bool found = Find(value, preds, succs);
            if (found)
            {
                return false;
            }
            else
            {
                var newNode = new SkipListNode<T>(topLevel, value);
                // Initialize each level pointer and span.
                for (int i = 0; i < topLevel; i++)
                {
                    newNode.Spans[i] = 1; // In a full implementation, compute the actual span.
                    newNode.Next[i].Set(succs[i], 0);
                }
                // Try to insert at level 0.
                int predStamp;
                SkipListNode<T> predNext = preds[0].Next[0].Get(out predStamp);
                if (!preds[0].Next[0].CompareAndSet(succs[0], newNode, predStamp, predStamp + 1))
                {
                    continue;
                }
                // Splice in the node at higher levels.
                for (int level = 1; level < topLevel; level++)
                {
                    bool splice = false;
                    while (!splice)
                    {
                        int predStampLevel;
                        SkipListNode<T> succ = preds[level].Next[level].Get(out predStampLevel);
                        if (preds[level].Next[level].CompareAndSet(succs[level], newNode, predStampLevel, predStampLevel + 1))
                        {
                            splice = true;
                        }
                        else
                        {
                            Find(value, preds, succs);
                        }
                    }
                }
                return true;
            }
        }
    }

    /// <summary>
    /// Removes a value from the skiplist.
    /// Returns true if the value was removed.
    /// </summary>
    public bool Remove(T value)
    {
        var preds = new SkipListNode<T>[MaxLevel];
        var succs = new SkipListNode<T>[MaxLevel];
        SkipListNode<T> nodeToRemove = null;
        bool isMarked = false;
        while (true)
        {
            bool found = Find(value, preds, succs);
            if (!found)
            {
                return false;
            }
            else
            {
                nodeToRemove = succs[0];
                if (Volatile.Read(ref nodeToRemove.Marked) == 0)
                {
                    if (Interlocked.CompareExchange(ref nodeToRemove.Marked, 1, 0) == 0)
                    {
                        isMarked = true;
                    }
                }
                if (!isMarked)
                {
                    return false;
                }
                // Physically remove the node from all levels.
                for (int level = nodeToRemove.Level - 1; level >= 0; level--)
                {
                    bool done = false;
                    while (!done)
                    {
                        int predStamp;
                        SkipListNode<T> predNext = preds[level].Next[level].Get(out predStamp);
                        int nodeStamp;
                        SkipListNode<T> nodeNext = nodeToRemove.Next[level].Get(out nodeStamp);
                        if (preds[level].Next[level].CompareAndSet(nodeToRemove, nodeNext, predStamp, predStamp + 1))
                        {
                            done = true;
                        }
                        else
                        {
                            Find(value, preds, succs);
                        }
                    }
                }
                return true;
            }
        }
    }

    /// <summary>
    /// Returns true if the skiplist contains the given value.
    /// </summary>
    public bool Contains(T value)
    {
        var preds = new SkipListNode<T>[MaxLevel];
        var succs = new SkipListNode<T>[MaxLevel];
        return Find(value, preds, succs);
    }

    /// <summary>
    /// Returns the value at the given index (based on spans).
    /// </summary>
    public T GetByIndex(int index)
    {
        SkipListNode<T> current = head;
        int traversed = 0;
        for (int level = MaxLevel - 1; level >= 0; level--)
        {
            int stamp;
            while (current.Next[level].Get(out stamp) != null && traversed + current.Spans[level] <= index)
            {
                traversed += current.Spans[level];
                current = current.Next[level].Get(out stamp);
            }
            if (traversed == index && current != head)
            {
                return current.Value;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(index), "Index not found");
    }

    /// <summary>
    /// Returns an enumerator that iterates over the skiplist in sorted order by traversing level 0.
    /// Only nodes that are not marked as deleted are yielded.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        int stamp;
        // Start at the first real node in level 0.
        SkipListNode<T> current = head.Next[0].Get(out stamp);
        while (current != null)
        {
            if (Volatile.Read(ref current.Marked) == 0)
            {
                yield return current.Value;
            }
            current = current.Next[0].Get(out stamp);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}