using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenAddressingHashTable<TKey, TValue> : IHashTable<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.6;

    private HashSlot<TKey, TValue>[] table;
    private int size;
    private int count;
    private readonly ProbingStrategy strategy;

    public OpenAddressingHashTable(ProbingStrategy strategy = ProbingStrategy.Linear)
    {
        this.strategy = strategy;
        size = DefaultCapacity;
        table = new HashSlot<TKey, TValue>[size];
    }

    public int Capacity => size;
    public int Count => count;
    public bool IsReadOnly => false;

    public bool isConflict {get; set;}
    public bool isResized { get; set;}

    // ── 해시 함수 ─────────────────────────────────────────────────────────

    public int GetHash(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int hash = key.GetHashCode();
        return (hash & 0x7fffffff) % size;
    }

    public int GetSecondaryHash(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int hash = key.GetHashCode();
        return 1 + ((hash & 0x7fffffff) % (size - 1));
    }

    // attempt번째 프로브 인덱스 반환
    public int GetProbeIndex(TKey key, int attempt)
    {
        Debug.Log($"[AddProbing] {key} - {attempt} : {strategy}");
        int h = GetHash(key);
        switch (strategy)
        {
            case ProbingStrategy.Linear:
                return (h + attempt) % size;
            case ProbingStrategy.Quadratic:
                return (h + attempt * attempt) % size;
            case ProbingStrategy.DoubleHash:
                return (h + attempt * GetSecondaryHash(key)) % size;
            default:
                return (h + attempt) % size;
        }
    }

    // ── 삽입 ──────────────────────────────────────────────────────────────

    public void Add(TKey key, TValue value)
    {
        isResized = false;
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (ContainsKey(key))
            throw new ArgumentException($"키가 이미 존재합니다: {key}");

        if ((double)count / size > LoadFactor)
            Resize();
            isResized = true;

        InsertInternal(table, size, key, value);
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    // ── 조회 ──────────────────────────────────────────────────────────────

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        for (int i = 0; i < size; i++)
        {
            int idx = GetProbeIndex(key, i);
            var slot = table[idx];

            if (slot.State == SlotState.Empty)
                break;

            if (slot.State == SlotState.Occupied && EqualityComparer<TKey>.Default.Equals(slot.Key, key))
            {
                value = slot.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
                return value;
            throw new KeyNotFoundException($"키를 찾을 수 없습니다: {key}");
        }
        set
        {
            isConflict = false;
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            for (int i = 0; i < size; i++)
            {
                int idx = GetProbeIndex(key, i);
                var slot = table[idx];

                if (slot.State == SlotState.Empty)
                    break;

                if (slot.State == SlotState.Occupied && EqualityComparer<TKey>.Default.Equals(slot.Key, key))
                {
                    table[idx].Value = value;
                    isConflict = true;
                    return;
                }
            }

            // 없으면 새로 삽입
            Add(key, value);
        }
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out TValue value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    // ── 삭제 ──────────────────────────────────────────────────────────────

    public bool Remove(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        for (int i = 0; i < size; i++)
        {
            int idx = GetProbeIndex(key, i);
            var slot = table[idx];

            if (slot.State == SlotState.Empty)
                return false;

            if (slot.State == SlotState.Occupied && EqualityComparer<TKey>.Default.Equals(slot.Key, key))
            {
                table[idx].State = SlotState.Deleted;   // Tombstone
                count--;
                return true;
            }
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item))
            return false;
        return Remove(item.Key);
    }

    // ── 초기화 ────────────────────────────────────────────────────────────

    public void Clear()
    {
        table = new HashSlot<TKey, TValue>[size];
        count = 0;
    }

    // ── 키/값 컬렉션 ──────────────────────────────────────────────────────

    public ICollection<TKey> Keys
    {
        get
        {
            var list = new List<TKey>();
            foreach (var slot in table)
                if (slot.State == SlotState.Occupied)
                    list.Add(slot.Key);
            return list;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var list = new List<TValue>();
            foreach (var slot in table)
                if (slot.State == SlotState.Occupied)
                    list.Add(slot.Value);
            return list;
        }
    }

    // ── 열거자 ────────────────────────────────────────────────────────────

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var slot in table)
            if (slot.State == SlotState.Occupied)
                yield return new KeyValuePair<TKey, TValue>(slot.Key, slot.Value);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        foreach (var slot in table)
        {
            if (slot.State == SlotState.Occupied)
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(slot.Key, slot.Value);
        }
    }

    // ── 시각화 스냅샷 ─────────────────────────────────────────────────────

    public HashTableSnapshot<TKey, TValue> GetSnapshot()
    {
        var slots = new HashSlot<TKey, TValue>[size];
        Array.Copy(table, slots, size);

        return new HashTableSnapshot<TKey, TValue>
        {
            TableType = "OpenAddressing",
            Capacity = size,
            Count = count,
            Slots = slots,
            Strategy = strategy
        };
    }

    // ── 내부 헬퍼 ─────────────────────────────────────────────────────────

    // 리사이징 후 재해싱 시에도 같은 로직을 재사용하기 위해 분리
    private void InsertInternal(HashSlot<TKey, TValue>[] targetTable, int targetSize, TKey key, TValue value)
    {
        for (int i = 0; i < targetSize; i++)
        {
            int idx = ProbeIndex(key, i, targetSize);
            if (targetTable[idx].State != SlotState.Occupied)
            {
                targetTable[idx] = new HashSlot<TKey, TValue>
                {
                    State = SlotState.Occupied,
                    Key = key,
                    Value = value
                };
                return;
            }
        }
        throw new InvalidOperationException("테이블이 가득 찼습니다.");
    }

    // InsertInternal 전용: targetSize를 직접 받아 인덱스 계산
    private int ProbeIndex(TKey key, int attempt, int targetSize)
    {
        int hash = (key.GetHashCode() & 0x7fffffff) % targetSize;
        switch (strategy)
        {
            case ProbingStrategy.Linear:
                return (hash + attempt) % targetSize;
            case ProbingStrategy.Quadratic:
                return (hash + attempt * attempt) % targetSize;
            case ProbingStrategy.DoubleHash:
                int h2 = 1 + ((key.GetHashCode() & 0x7fffffff) % (targetSize - 1));
                return (hash + attempt * h2) % targetSize;
            default:
                return (hash + attempt) % targetSize;
        }
    }

    private void Resize()
    {
        int newSize = size * 2;
        var newTable = new HashSlot<TKey, TValue>[newSize];

        foreach (var slot in table)
        {
            if (slot.State == SlotState.Occupied)
            {
                // newSize 기준으로 재해싱
                int oldSize = size;
                size = newSize;
                InsertInternal(newTable, newSize, slot.Key, slot.Value);
                size = oldSize;
            }
        }

        size = newSize;
        table = newTable;
        // count는 Deleted 슬롯을 제외하므로 그대로 유지
    }
}
