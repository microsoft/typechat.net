// Copyright (c) Microsoft. All rights reserved.

using System.Reflection.Metadata.Ecma335;

namespace Microsoft.TypeChat.Schema;

public abstract class TypeExporter<T>
{
    HashSet<T> _exportedTypes;
    Queue<T>? _pendingTypes;

    public TypeExporter()
    {
        _exportedTypes = new HashSet<T>();
    }

    public virtual void Clear()
    {
        _exportedTypes?.Clear();
        _pendingTypes?.Clear();
    }


    public void AddPending(T type)
    {
        if (!IsExported(type) && ShouldExport(type))
        {
            _pendingTypes ??= new Queue<T>();
            _pendingTypes.Enqueue(type);
        }
    }

    public void AddPending(IEnumerable<T> types)
    {
        foreach (var t in types)
        {
            AddPending(t);
        }
    }

    public T? GetPending()
    {
        if (_pendingTypes != null && _pendingTypes.Count > 0)
        {
            return _pendingTypes.Dequeue();
        }

        return default;
    }

    public bool IsExported(T type) => _exportedTypes.Contains(type);

    protected void AddExported(T type)
    {
        _exportedTypes.Add(type);
    }

    public void ExportQueued()
    {
        T? type;
        while ((type = GetPending()) != null)
        {
            Export(type);
        }
    }

    protected virtual bool ShouldExport(T t) => true;
    public abstract void Export(T t);
}
