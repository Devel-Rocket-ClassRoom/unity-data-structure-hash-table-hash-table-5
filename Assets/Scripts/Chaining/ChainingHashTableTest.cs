using System.Text;
using UnityEngine;

public class ChainingHashTableTest : MonoBehaviour
{
    public int key;
    public int value;

    private ChainingHashTable<int, int> table = new ChainingHashTable<int, int>(16);

    [ContextMenu("Add")]
    void Add()
    {
        try
        {
            table.Add(key, value);
            Debug.Log($"[Add] key={key}, value={value} 성공 | Count={table.Count}");
        }
        catch (System.ArgumentException)
        {
            Debug.LogWarning($"[Add] 실패 - key={key} 이미 존재");
        }
        PrintTable();
    }

    [ContextMenu("Remove")]
    void Remove()
    {
        bool ok = table.Remove(key);
        Debug.Log(ok
            ? $"[Remove] key={key} 삭제 성공 | Count={table.Count}"
            : $"[Remove] 실패 - key={key} 없음");
        PrintTable();
    }

    [ContextMenu("Get")]
    void Get()
    {
        if (table.TryGetValue(key, out int v))
            Debug.Log($"[Get] key={key} → value={v}");
        else
            Debug.LogWarning($"[Get] 실패 - key={key} 없음");
    }

    [ContextMenu("Contains")]
    void Contains()
    {
        Debug.Log($"[Contains] key={key} → {(table.ContainsKey(key) ? "존재" : "없음")}");
    }

    [ContextMenu("Clear")]
    void Clear()
    {
        table.Clear();
        Debug.Log("[Clear] 초기화 완료");
        PrintTable();
    }

    [ContextMenu("Collision Test")]
    void CollisionTest()
    {
        table.Clear();
        // capacity=16이므로 key % 16이 같은 키는 같은 버킷으로 충돌
        // 0%16=0, 16%16=0, 32%16=0 → 세 키 모두 bucket[0]으로 충돌 → 체이닝 발생
        int[] collidingKeys = { 0, 16, 32 };
        foreach (int k in collidingKeys)
        {
            table.Add(k, k * 10);
            Debug.Log($"  [CollisionTest] Add key={k} → bucket[{k % 16}]");
        }

        PrintTable();

        // 체이닝 탐색 검증: bucket[0] 체인 안에서 key=16 찾기
        if (table.TryGetValue(16, out int val))
            Debug.Log($"[CollisionTest] 체이닝 탐색 성공 - key=16, value={val} (bucket[0] 체인에서 발견)");
        else
            Debug.LogError("[CollisionTest] 체이닝 탐색 실패");
    }

    [ContextMenu("Print Table")]
    void PrintTable()
    {
        var snap = table.GetSnapshot();
        var sb = new StringBuilder();
        sb.AppendLine($"[Table] Capacity={snap.Capacity} Count={snap.Count}");
        for (int i = 0; i < snap.Capacity; i++)
        {
            var slot = snap.Slots[i];
            if (slot.State == SlotState.Occupied && slot.Chain != null)
            {
                sb.Append($"  [{i:D2}] ");
                foreach (var item in slot.Chain)
                    sb.Append($"({item.Key}:{item.Value}) -> ");
                sb.Append("null");
                sb.AppendLine();
            }
        }
        Debug.Log(sb.ToString());
    }
}
