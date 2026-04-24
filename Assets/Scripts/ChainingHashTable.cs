using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainingHashTable<TKey, TValue> : IHashTable<TKey, TValue>
{
    // 초기 배열 크기
    private const int DefaultCapacity = 16;
    // 이 비율을 넘으면 Resize 실행 (count / Capacity > 0.75)
    private const float LoadFactor = 0.75f;

    // 슬롯 배열 — 각 슬롯은 체인(List)을 가짐
    private HashSlot<TKey, TValue>[] buckets;
    // 현재 저장된 원소 수
    private int count;

    public ChainingHashTable(int capacity = DefaultCapacity)
    {
        buckets = new HashSlot<TKey, TValue>[capacity];
    }

    
    public int Capacity => buckets.Length;
    
    public int Count => count;
  
    public bool IsReadOnly => false;
    public bool isConflict {get; set;}
    public bool isResized { get; set;}

    // 저장된 모든 키 목록
    public ICollection<TKey> Keys
    {
        get
        {
            // 모든 슬롯의 체인을 순회해서 키만 모아 반환
            var keys = new List<TKey>();
            foreach (var slot in buckets)
            {
                if (slot.Chain == null) continue;
                foreach (var item in slot.Chain)
                    keys.Add(item.Key);
            }
            return keys;
        }
    }

    // 저장된 모든 값 목록
    public ICollection<TValue> Values
    {
        get
        {
            // 모든 슬롯의 체인을 순회해서 값만 모아 반환
            var values = new List<TValue>();
            foreach (var slot in buckets)
            {
                if (slot.Chain == null) continue;
                foreach (var item in slot.Chain)
                    values.Add(item.Value);
            }
            return values;
        }
    }

    // 키로 값을 읽거나 쓰는 인덱서
    public TValue this[TKey key]
    {
        get
        {
            // 키로 값 탐색, 없으면 예외
            if (TryGetValue(key, out TValue value))
                return value;
            throw new KeyNotFoundException($"키를 찾을 수 없습니다: {key}");
        }
        set
        {
            isConflict = false;
            int index = GetHash(key);
            // 체인이 있으면 기존 키 탐색해서 값 교체
            if (buckets[index].Chain != null)
            {
                isConflict = true;
                for (int i = 0; i < buckets[index].Chain.Count; i++)
                {
                    if (buckets[index].Chain[i].Key.Equals(key))
                    {
                        buckets[index].Chain[i] = (key, value);
                        return;
                    }
                }
            }
            // 없으면 새로 추가
            Add(key, value);
        }
    }

    // 키 → 배열 인덱스 변환 (절댓값 후 Capacity로 나머지)
    public int GetHash(TKey key)
    {
        return Math.Abs(key.GetHashCode()) % Capacity;
    }

    // 시각화용 현재 상태 스냅샷 반환
    public HashTableSnapshot<TKey, TValue> GetSnapshot()
    {
        // 현재 buckets 배열을 복사해서 스냅샷 구성
        var slots = new HashSlot<TKey, TValue>[Capacity];
        for (int i = 0; i < Capacity; i++)
        {
            slots[i] = buckets[i];
            // 체인이 있으면 새 리스트로 복사 (원본 보호)
            if (buckets[i].Chain != null)
                slots[i].Chain = new List<(TKey Key, TValue Value)>(buckets[i].Chain);
        }
        return new HashTableSnapshot<TKey, TValue>
        {
            TableType = "Chaining",
            Capacity = Capacity,
            Count = count,
            Slots = slots,
            Strategy = null
        };
    }

    // 키-값 추가 (중복 키면 예외)
    public void Add(TKey key, TValue value)
    {
        isResized = false;
        // 키를 배열 인덱스로 변환
        int index = GetHash(key);

        // 해당 슬롯에 체인이 없으면 새로 생성
        if (buckets[index].Chain == null)
            buckets[index].Chain = new List<(TKey Key, TValue Value)>();

        // 체인 순회 — 같은 키가 이미 있으면 예외
        foreach (var item in buckets[index].Chain)
        {
            if (item.Key.Equals(key))
                throw new ArgumentException($"이미 존재하는 키입니다: {key}");
        }

        // 체인 끝에 새 항목 추가
        buckets[index].Chain.Add((key, value));
        // 슬롯 상태를 Occupied로 표시
        buckets[index].State = SlotState.Occupied;
        // 원소 수 증가
        count++;

        // 로드팩터 초과 시 배열 크기 2배로 확장
        if ((float)count / Capacity > LoadFactor)
            Resize();
            isResized = true;
    }

    // KeyValuePair 버전 Add — 내부적으로 Add(key, value) 호출
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    // 키로 원소 제거, 성공 여부 반환
    public bool Remove(TKey key)
    {
        // 키를 배열 인덱스로 변환
        int index = GetHash(key);

        // 체인이 없으면 해당 슬롯에 아무것도 없는 것
        if (buckets[index].Chain == null)
            return false;

        // foreach 대신 for — 순회 중 삭제 가능
        for (int i = 0; i < buckets[index].Chain.Count; i++)
        {
            if (buckets[index].Chain[i].Key.Equals(key))
            {
                // 해당 항목 체인에서 제거
                buckets[index].Chain.RemoveAt(i);

                // 체인이 비었으면 슬롯 상태를 Empty로
                if (buckets[index].Chain.Count == 0)
                    buckets[index].State = SlotState.Empty;

                count--;
                return true;
            }
        }

        // 끝까지 못 찾으면 실패
        return false;
    }

    // KeyValuePair 버전 Remove — 내부적으로 Remove(key) 호출
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    // 키로 값 탐색, 찾으면 out value에 담고 true 반환
    public bool TryGetValue(TKey key, out TValue value)
    {
        // 키를 배열 인덱스로 변환
        int index = GetHash(key);

        // 체인이 없으면 해당 슬롯에 아무것도 없는 것
        if (buckets[index].Chain == null)
        {
            value = default;
            return false;
        }

        // 체인 순회하며 키 탐색
        foreach (var item in buckets[index].Chain)
        {
            if (item.Key.Equals(key))
            {
                // 찾으면 값 반환
                value = item.Value;
                return true;
            }
        }

        // 끝까지 못 찾으면 실패
        value = default;
        return false;
    }

    // 키 존재 여부 확인 — TryGetValue 결과만 반환
    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    // KeyValuePair 존재 여부 확인 — ContainsKey 활용
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    // 모든 원소 제거 — 배열을 새로 만들고 count 초기화
    public void Clear()
    {
        buckets = new HashSlot<TKey, TValue>[Capacity];
        count = 0;
    }

    // 전체 원소를 array[arrayIndex]부터 복사
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        // GetEnumerator로 순회하며 배열에 순서대로 담음
        foreach (var pair in this)
            array[arrayIndex++] = pair;
    }

    // 전체 원소를 순회하는 열거자
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        // 모든 슬롯의 체인을 순서대로 순회
        foreach (var slot in buckets)
        {
            if (slot.Chain == null) continue;
            foreach (var item in slot.Chain)
                yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
        }
    }

    // 비제네릭 열거자 — GetEnumerator() 위임
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // 배열 크기를 2배로 늘리고 전체 재해시
    private void Resize()
    {
        // 2배 크기의 새 배열 생성
        var newBuckets = new HashSlot<TKey, TValue>[Capacity * 2];

        Debug.Log($"리사이즈 호출 {Capacity}");


        // 기존 모든 원소를 새 배열에 재해시해서 삽입
        foreach (var slot in buckets)
        {
            if (slot.Chain == null) continue;
            foreach (var item in slot.Chain)
            {
                // 새 Capacity 기준으로 인덱스 재계산
                int newIndex = Math.Abs(item.Key.GetHashCode()) % newBuckets.Length;
                if (newBuckets[newIndex].Chain == null)
                    newBuckets[newIndex].Chain = new List<(TKey Key, TValue Value)>();
                newBuckets[newIndex].Chain.Add(item);
                newBuckets[newIndex].State = SlotState.Occupied;
            }
        }

        // 새 배열로 교체
        buckets = newBuckets;
    }
}
