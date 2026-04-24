using System;
using System.Collections;
using System.Collections.Generic;

public class SimpleHashTable<TKey, TValue> : IHashTable<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const float LoadFactor = 0.75f;

    private HashSlot<TKey, TValue>[] table;
    private int count;
    public bool isConflict { get; set; }
    public bool isResized { get; set;}
    public SimpleHashTable(int capacity = DefaultCapacity)
    {
        table = new HashSlot<TKey, TValue>[capacity];
    }

    public int Capacity => table.Length;
    public int Count => count;
    public bool IsReadOnly => false;

    public int GetHash(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return (key.GetHashCode() & 0x7fffffff) % Capacity;
    }

    public void Add(TKey key, TValue value)
    {
        isResized = false;
        if (key == null) throw new ArgumentNullException(nameof(key));

        if ((float)count / Capacity >= LoadFactor)
            Resize();
            isResized = true;

        int idx = GetHash(key);

        if (table[idx].State == SlotState.Occupied)
        {
            if (EqualityComparer<TKey>.Default.Equals(table[idx].Key, key))
                throw new ArgumentException($"키가 이미 존재합니다: {key}");
            throw new InvalidOperationException($"해시 충돌: 인덱스 {idx}에 다른 키가 존재합니다.");
        }

        table[idx] = new HashSlot<TKey, TValue>
        {
            State = SlotState.Occupied,
            Key = key,
            Value = value
        };
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        int idx = GetHash(key);
        var slot = table[idx];

        if (slot.State == SlotState.Occupied &&
            EqualityComparer<TKey>.Default.Equals(slot.Key, key))
        {
            value = slot.Value;
            return true;
        }

        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue val)) return val;
            throw new KeyNotFoundException($"키를 찾을 수 없습니다: {key}");
        }
        set
        {
            isConflict = false;
            if (key == null) throw new ArgumentNullException(nameof(key));
            int idx = GetHash(key);
            if (table[idx].State == SlotState.Occupied &&
                EqualityComparer<TKey>.Default.Equals(table[idx].Key, key))
            {
                table[idx].Value = value;
                isConflict = true;
                return;
            }
            Add(key, value);
        }
    }

    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out TValue val)) return false;
        return EqualityComparer<TValue>.Default.Equals(val, item.Value);
    }

    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        int idx = GetHash(key);
        var slot = table[idx];

        if (slot.State == SlotState.Occupied &&
            EqualityComparer<TKey>.Default.Equals(slot.Key, key))
        {
            table[idx] = default;
            count--;
            return true;
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item)) return false;
        return Remove(item.Key);
    }

    public void Clear()
    {
        table = new HashSlot<TKey, TValue>[Capacity];
        count = 0;
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var list = new List<TKey>();
            foreach (var s in table)
                if (s.State == SlotState.Occupied) list.Add(s.Key);
            return list;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var list = new List<TValue>();
            foreach (var s in table)
                if (s.State == SlotState.Occupied) list.Add(s.Value);
            return list;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var s in table)
            if (s.State == SlotState.Occupied)
                yield return new KeyValuePair<TKey, TValue>(s.Key, s.Value);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        foreach (var s in table)
            if (s.State == SlotState.Occupied)
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(s.Key, s.Value);
    }

    public HashTableSnapshot<TKey, TValue> GetSnapshot()
    {
        var slots = new HashSlot<TKey, TValue>[Capacity];
        Array.Copy(table, slots, Capacity);
        return new HashTableSnapshot<TKey, TValue>
        {
            TableType = "Simple",
            Capacity = Capacity,
            Count = count,
            Slots = slots,
            Strategy = null
        };
    }

    private void Resize()
    {
        var newTable = new HashSlot<TKey, TValue>[Capacity * 2];
        foreach (var s in table)
        {
            if (s.State != SlotState.Occupied) continue;
            int idx = (s.Key.GetHashCode() & 0x7fffffff) % newTable.Length;
            newTable[idx] = s;
        }
        table = newTable;
    }
}
