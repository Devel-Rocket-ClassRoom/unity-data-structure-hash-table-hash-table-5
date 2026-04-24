using UnityEngine;

public class OpenAddressingTest : MonoBehaviour
{
    [Header("테스트 설정")]
    public ProbingStrategy strategy = ProbingStrategy.Linear;
    public bool runOnStart = true;

    void Start()
    {
        if (runOnStart)
            RunAllTests();
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("========== OpenAddressing 테스트 시작 ==========");
        TestAdd();
        TestGet();
        TestContains();
        TestRemoveAndTombstone();
        TestUpdate();
        TestResize();
        TestAllStrategies();
        Debug.Log("========== 모든 테스트 완료 ==========");
    }

    void Pass(string testName) => Debug.Log($"[PASS] {testName}");
    void Fail(string testName, string reason) => Debug.LogError($"[FAIL] {testName} - {reason}");

    void Assert(bool condition, string testName, string failReason = "조건 불만족")
    {
        if (condition) Pass(testName);
        else Fail(testName, failReason);
    }

    // ── Add ───────────────────────────────────────────────────────────────

    void TestAdd()
    {
        var ht = new OpenAddressingHashTable<string, int>(strategy);
        ht.Add("apple", 1);
        ht.Add("banana", 2);
        ht.Add("cherry", 3);

        Assert(ht.Count == 3, "Add: Count 확인");

        bool threw = false;
        try { ht.Add("apple", 99); }
        catch (System.ArgumentException) { threw = true; }
        Assert(threw, "Add: 중복 키 예외 발생");
    }

    // ── Get ───────────────────────────────────────────────────────────────

    void TestGet()
    {
        var ht = new OpenAddressingHashTable<string, int>(strategy);
        ht.Add("hp", 100);
        ht.Add("mp", 50);

        Assert(ht["hp"] == 100, "Get: 키 hp 조회");
        Assert(ht["mp"] == 50,  "Get: 키 mp 조회");

        bool threw = false;
        try { _ = ht["nonexistent"]; }
        catch (System.Collections.Generic.KeyNotFoundException) { threw = true; }
        Assert(threw, "Get: 없는 키 예외 발생");

        Assert(ht.TryGetValue("hp", out int val) && val == 100, "TryGetValue: 성공 케이스");
        Assert(!ht.TryGetValue("xyz", out _), "TryGetValue: 실패 케이스");
    }

    // ── ContainsKey ────────────────────────────────────────────────────────

    void TestContains()
    {
        var ht = new OpenAddressingHashTable<int, string>(strategy);
        ht.Add(1, "one");
        ht.Add(2, "two");

        Assert(ht.ContainsKey(1),  "ContainsKey: 존재하는 키");
        Assert(!ht.ContainsKey(9), "ContainsKey: 없는 키");
    }

    // ── Remove + Tombstone ────────────────────────────────────────────────

    void TestRemoveAndTombstone()
    {
        var ht = new OpenAddressingHashTable<string, int>(strategy);
        ht.Add("a", 1);
        ht.Add("b", 2);
        ht.Add("c", 3);

        bool removed = ht.Remove("b");
        Assert(removed, "Remove: 존재하는 키 삭제 반환값");
        Assert(ht.Count == 2, "Remove: Count 감소 확인");
        Assert(!ht.ContainsKey("b"), "Remove: 삭제된 키 조회 불가");

        // Tombstone 이후에도 c 탐색 가능한지 확인
        Assert(ht.ContainsKey("c"), "Remove(Tombstone): 삭제 후 c 탐색 유지");

        Assert(!ht.Remove("zzz"), "Remove: 없는 키 삭제 반환값 false");

        // 삭제 후 같은 키 재삽입
        ht.Add("b", 99);
        Assert(ht["b"] == 99, "Remove 후 재삽입: 값 확인");
    }

    // ── 인덱서 Update ──────────────────────────────────────────────────────

    void TestUpdate()
    {
        var ht = new OpenAddressingHashTable<string, int>(strategy);
        ht.Add("score", 0);
        ht["score"] = 500;

        Assert(ht["score"] == 500, "Update: 인덱서로 값 갱신");
        Assert(ht.Count == 1, "Update: Count 변화 없음");
    }

    // ── Resize ────────────────────────────────────────────────────────────

    void TestResize()
    {
        var ht = new OpenAddressingHashTable<int, int>(strategy);
        int beforeCap = ht.Capacity;

        // LoadFactor(0.6) 초과 → 리사이즈 유발
        for (int i = 0; i < 20; i++)
            ht.Add(i, i * 10);

        Assert(ht.Capacity > beforeCap, "Resize: 용량 증가 확인");
        Assert(ht.Count == 20, "Resize: 데이터 손실 없음");

        for (int i = 0; i < 20; i++)
            if (ht[i] != i * 10) { Fail("Resize: 재해싱 값 검증", $"키 {i} 오류"); return; }
        Pass("Resize: 재해싱 후 모든 값 정상");
    }

    // ── 전략별 비교 ───────────────────────────────────────────────────────

    void TestAllStrategies()
    {
        foreach (ProbingStrategy s in System.Enum.GetValues(typeof(ProbingStrategy)))
        {
            var ht = new OpenAddressingHashTable<string, int>(s);
            ht.Add("x", 1);
            ht.Add("y", 2);
            ht.Add("z", 3);
            ht.Remove("y");
            ht.Add("w", 4);

            bool ok = ht.Count == 3 && ht["x"] == 1 && ht["z"] == 3 && ht["w"] == 4;
            Assert(ok, $"Strategy({s}): 삽입/삭제/조회 종합");
        }
    }
}
