using UnityEngine;

public class SimpleHashTableTest : MonoBehaviour
{
    [Header("테스트 설정")]
    public bool runOnStart = true;

    void Start()
    {
        if (runOnStart)
            RunAllTests();
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("========== SimpleHashTable 테스트 시작 ==========");
        TestAdd();
        TestGet();
        TestContains();
        TestRemove();
        TestUpdate();
        TestCollision();
        TestResize();
        Debug.Log("========== 모든 테스트 완료 ==========");
    }

    void Pass(string name) => Debug.Log($"[PASS] {name}");
    void Fail(string name, string reason) => Debug.LogError($"[FAIL] {name} - {reason}");

    void Assert(bool condition, string name, string failReason = "조건 불만족")
    {
        if (condition) Pass(name);
        else Fail(name, failReason);
    }

    void TestAdd()
    {
        var ht = new SimpleHashTable<string, int>();
        ht.Add("hp", 100);
        ht.Add("mp", 50);

        Assert(ht.Count == 2, "Add: Count 확인");

        // 중복 키
        bool threw = false;
        try { ht.Add("hp", 999); }
        catch (System.ArgumentException) { threw = true; }
        Assert(threw, "Add: 중복 키 예외(ArgumentException)");
    }


    void TestGet()
    {
        var ht = new SimpleHashTable<string, int>();
        ht.Add("gold", 500);

        Assert(ht["gold"] == 500, "Get: 인덱서 조회");
        Assert(ht.TryGetValue("gold", out int v) && v == 500, "TryGetValue: 성공");
        Assert(!ht.TryGetValue("none", out _), "TryGetValue: 실패");

        bool threw = false;
        try { _ = ht["none"]; }
        catch (System.Collections.Generic.KeyNotFoundException) { threw = true; }
        Assert(threw, "Get: 없는 키 예외(KeyNotFoundException)");
    }

    
    void TestContains()
    {
        var ht = new SimpleHashTable<int, string>();
        ht.Add(1, "one");

        Assert(ht.ContainsKey(1),  "ContainsKey: 존재하는 키");
        Assert(!ht.ContainsKey(9), "ContainsKey: 없는 키");
    }

    
    void TestRemove()
    {
        var ht = new SimpleHashTable<string, int>();
        ht.Add("a", 1);
        ht.Add("b", 2);

        Assert(ht.Remove("a"),    "Remove: 존재하는 키 반환값 true");
        Assert(ht.Count == 1,     "Remove: Count 감소");
        Assert(!ht.ContainsKey("a"), "Remove: 삭제 후 조회 불가");
        Assert(!ht.Remove("zzz"), "Remove: 없는 키 반환값 false");

        // 삭제 후 같은 슬롯에 재삽입
        ht.Add("a", 99);
        Assert(ht["a"] == 99, "Remove 후 재삽입: 값 확인");
    }

    
    void TestUpdate()
    {
        var ht = new SimpleHashTable<string, int>();
        ht.Add("score", 0);
        ht["score"] = 300;

        Assert(ht["score"] == 300, "Update: 인덱서로 값 갱신");
        Assert(ht.Count == 1,      "Update: Count 변화 없음");
    }


    void TestCollision()
    {
        var ht = new SimpleHashTable<int, string>(16);
        ht.Add(0, "zero");

        bool threw = false;
        try { ht.Add(16, "sixteen"); }   // 0과 동일 슬롯
        catch (System.InvalidOperationException) { threw = true; }
        Assert(threw, "Collision: 충돌 시 예외(InvalidOperationException)");

        Assert(ht.Count == 1, "Collision: 충돌 후 Count 유지");
        Assert(ht.TryGetValue(0, out _), "Collision: 기존 키 정상 조회");
    }

    
    void TestResize()
    {
        var ht = new SimpleHashTable<int, int>();
        int beforeCap = ht.Capacity;

        // LoadFactor초과 유발
        for (int i = 0; i < 13; i++)
            ht.Add(i, i * 10);

        Assert(ht.Capacity > beforeCap, "Resize: 용량 증가");
        Assert(ht.Count == 13,           "Resize: 데이터 손실 없음");

        for (int i = 0; i < 13; i++)
            if (ht[i] != i * 10) { Fail("Resize: 재배치 값 검증", $"키 {i} 오류"); return; }
        Pass("Resize: 재배치 후 모든 값 정상");
    }
}
