// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

internal struct MinHeap<T>
{
    private static readonly T[] s_emptyBuffer = Array.Empty<T>();

    private readonly ScoredValue<T>[] _items;
    private int _count;

    public MinHeap(int capacity)
    {
        _items = new ScoredValue<T>[capacity + 1];
        _count = 0;
        //
        // The 0'th item is a sentinel entry that simplifies the code
        //
        _items[0] = new ScoredValue<T>(default, double.MinValue);
    }

    public int Count => _count;

    public int Capacity => _items.Length - 1; // 0'th item is always a sentinel to simplify code

    public ScoredValue<T> this[int index]
    {
        get => _items[index + 1];
        internal set { this._items[index + 1] = value; }
    }

    public ScoredValue<T> Top => _items[1];

    public bool IsEmpty => (this._count == 0);

    public void Clear()
    {
        this._count = 0;
    }

    public void Add(ScoredValue<T> item)
    {
        //
        // the 0'th item is always a sentinel and not included in this._count.
        // The length of the buffer is always this._count + 1
        //
        if (_count == Capacity)
        {
            throw new ArgumentOutOfRangeException($"Heap stores max {Capacity} entries");
        }
        _count++;
        _items[_count] = item;
        this.UpHeap(this._count);
    }

    public ScoredValue<T> RemoveTop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("MinHeap is empty.");
        }

        ScoredValue<T> item = _items[1];
        _items[1] = _items[_count--];
        DownHeap(1);
        return item;
    }

    public IEnumerable<ScoredValue<T>> RemoveAll()
    {
        while (this._count > 0)
        {
            yield return this.RemoveTop();
        }
    }

    private void UpHeap(int startAt)
    {
        int i = startAt;
        ScoredValue<T>[] items = _items;
        ScoredValue<T> item = items[i];
        int parent = i >> 1; //i / 2;

        while (parent > 0 && items[parent].CompareTo(item) > 0)
        {
            // Child > parent. Exchange with parent, thus moving the child up the queue
            items[i] = items[parent];
            i = parent;
            parent = i >> 1; //i / 2;
        }

        items[i] = item;
    }

    private void DownHeap(int startAt)
    {
        int i = startAt;
        int count = this._count;
        int maxParent = count >> 1;
        ScoredValue<T>[] items = _items;
        ScoredValue<T> item = items[i];

        while (i <= maxParent)
        {
            int child = i + i;
            //
            // Exchange the item with the smaller of its two children - if one is smaller, i.e.
            //
            // First, find the smaller child
            //
            if (child < count && items[child].CompareTo(items[child + 1]) > 0)
            {
                child++;
            }

            if (item.CompareTo(items[child]) <= 0)
            {
                // Heap condition is satisfied. Parent <= both its children
                break;
            }

            // Else, swap parent with the smallest child
            items[i] = items[child];
            i = child;
        }

        items[i] = item;
    }

    /// <summary>
    /// Heap Sort in-place.
    /// This is destructive. Once you do this, the heap order is lost.
    /// The advantage on in-place is that we don't need to do another allocation
    /// </summary>
    public void SortDescending()
    {
        int count = this._count;
        int i = count; // remember that the 0'th item in the queue is always a sentinel. So i is 1 based

        while (this._count > 0)
        {
            //
            // this dequeues the item with the current LOWEST relevancy
            // We take that and place it at the 'back' of the array - thus inverting it
            //
            ScoredValue<T> item = RemoveTop();
            _items[i--] = item;
        }

        _count = count;
    }
}
