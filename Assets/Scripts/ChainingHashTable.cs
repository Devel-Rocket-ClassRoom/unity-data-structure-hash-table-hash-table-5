using System;
using System.Collections;
using System.Collections.Generic;

public class ChainingHashTable<TKey, TValue> : IHashTable<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const float LoadFactor = 0.75f;

    private HashSlot<TKey, TValue>[] buckets;
    private int count;

    public ChainingHashTable(int capacity = DefaultCapacity)
    {
        buckets = new HashSlot<TKey, TValue>[capacity];
    }

    public int Capacity => buckets.Length;
    public int Count => count;
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => throw new NotImplementedException();
    public ICollection<TValue> Values => throw new NotImplementedException();

    public TValue this[TKey key]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int GetHash(TKey key)
    {
        throw new NotImplementedException();
    }

    public HashTableSnapshot<TKey, TValue> GetSnapshot()
    {
        throw new NotImplementedException();
    }

    public void Add(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Resize()
    {
        throw new NotImplementedException();
    }
}
