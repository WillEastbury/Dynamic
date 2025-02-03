// A helper class that couples a reference with an integer stamp (version).
public class AtomicStampedReference<T> where T : class
{
    // Internal container for a reference and its stamp.
    private class ReferenceWithStamp
    {
        public T Reference;
        public int Stamp;
        public ReferenceWithStamp(T reference, int stamp)
        {
            Reference = reference;
            Stamp = stamp;
        }
    }
    
    // Volatile ensures that _refWithStamp is always read fresh.
    private volatile ReferenceWithStamp _refWithStamp;
    
    public AtomicStampedReference(T initialRef, int initialStamp)
    {
        _refWithStamp = new ReferenceWithStamp(initialRef, initialStamp);
    }
    
    // Returns the current reference and outputs its stamp.
    public T Get(out int stamp)
    {
        var rws = _refWithStamp;
        stamp = rws.Stamp;
        return rws.Reference;
    }
    
    // Atomically sets the reference and stamp if both match the expected values.
    public bool CompareAndSet(T expectedReference, T newReference, int expectedStamp, int newStamp)
    {
        var current = _refWithStamp;
        if (current.Reference == expectedReference && current.Stamp == expectedStamp)
        {
            var newRef = new ReferenceWithStamp(newReference, newStamp);
            return Interlocked.CompareExchange(ref _refWithStamp, newRef, current) == current;
        }
        return false;
    }
    
    // Sets the reference and stamp unconditionally.
    public void Set(T newReference, int newStamp)
    {
        _refWithStamp = new ReferenceWithStamp(newReference, newStamp);
    }
}
