// A helper class that couples a reference with an integer stamp (version).
// Our SkipList node now uses stamped references for its Next pointers.
public class SkipListNode<T> where T : IComparable<T>
{
    public T Value;
    // Each level's pointer is an AtomicStampedReference to help solve the ABA problem.
    public AtomicStampedReference<SkipListNode<T>>[] Next;
    // Span values for indexed access. (This example leaves span updates simplified.)
    public int[] Spans;
    // Marking field: 0 means not marked; 1 means logically deleted.
    public int Marked;

    public SkipListNode(int level, T value)
    {
        Value = value;
        Next = new AtomicStampedReference<SkipListNode<T>>[level];
        Spans = new int[level];
        Marked = 0;
        for (int i = 0; i < level; i++)
        {
            // Initialize each pointer to null with stamp 0.
            Next[i] = new AtomicStampedReference<SkipListNode<T>>(null, 0);
            Spans[i] = 0; // Spans are initialized to 0 here.
        }
    }

    public int Level => Next.Length;
}
