using System.Collections.Generic;


// ── OpenAddressingHashTable 전용 ─────────────────────────────────────────
public enum ProbingStrategy { Linear, Quadratic, DoubleHash }


// ── 슬롯 상태 ────────────────────────────────────────────────────────────
// Simple       : Empty / Occupied (occupied[] 대신 State로 통일)
// Chaining     : Empty / Occupied
// OpenAddressing : Empty / Occupied / Deleted (Tombstone)
public enum SlotState { Empty, Occupied, Deleted }


// ── 슬롯 하나 ────────────────────────────────────────────────────────────
public struct HashSlot<TKey, TValue>
{
    public SlotState State;
    public TKey Key;
    public TValue Value;

    // Chaining 전용 — Simple / OpenAddressing 은 null
    public List<(TKey Key, TValue Value)>? Chain;
}


// ── 시각화용 스냅샷 ──────────────────────────────────────────────────────
public struct HashTableSnapshot<TKey, TValue>
{
    public string TableType;   // "Simple" / "Chaining" / "OpenAddressing"
    public int Capacity;       // 내부 배열 크기
    public int Count;          // 현재 원소 수
    public HashSlot<TKey, TValue>[] Slots;

    // OpenAddressing 전용 — Simple / Chaining 은 null
    public ProbingStrategy? Strategy;
}


// ── 공용 인터페이스 ──────────────────────────────────────────────────────
// 이걸 구현하실 때 쓰세요!
// GetSnapshot() / GetHash() / Capacity 가 빠지면 컴파일 에러.
public interface IHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    // 시각화 담당자(A)가 호출하는 유일한 창구
    HashTableSnapshot<TKey, TValue> GetSnapshot();

    // 키 → 인덱스 변환 (디버깅 / 시각화에서 직접 확인용)
    int GetHash(TKey key);

    // 내부 배열 크기
    int Capacity { get; }
}
