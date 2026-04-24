using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections;

// 더미데이터 활용
public class UiHashTableTest : MonoBehaviour
{
    // --- 인스펙터 연결 필드 ---
    public UiHashSlot prefab;
    public TextMeshProUGUI logText;
    public Transform slotContent;
    public TMP_Dropdown dropdown;
    public TMP_Dropdown openDropdown;
    public TMP_InputField keyInputField;
    public TMP_InputField valueInputField;
    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    // --- 내부 사용 필드 ---
    private HashTableSnapshot<string, int> snapShot;
    private IHashTable<string, int> table;
    private int modeIndex; // clear후 다시 생성할 때 mode 기록용
    private string selectedKey;
    private StringBuilder logs;
    private int capacity;

    private void Awake()
    {
        selectedKey = string.Empty;
        table = new SimpleHashTable<string, int>();
        snapShot = table.GetSnapshot();
        logs = new StringBuilder();
        logText.text = logs.ToString();
        capacity = table.Capacity;

        dropdown.onValueChanged.AddListener(OnModeChanged);
        openDropdown.onValueChanged.AddListener(OnProbeModeChanged);
        addButton.onClick.AddListener(OnAddSlot);
        clearButton.onClick.AddListener(OnClearSlot);
        removeButton.onClick.AddListener(OnRemoveSlot);
    }
    private void Start()
    {
        VisualizeSlots();
    }

    private void OnModeChanged(int index)
    {
        int count = slotContent.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            Destroy(slotContent.GetChild(i).gameObject);
        }

        switch (index)
        {
            case 0:
                modeIndex = index;
                table = new SimpleHashTable<string, int>();
                capacity = table.Capacity;
                logs.Append("Change Type of HashTable: Simple\n");
                break;
            case 1: 
                modeIndex = index;
                table = new ChainingHashTable<string, int>();
                capacity = table.Capacity;
                logs.Append("Change Type of HashTable: Chaining\n");
                break;
            case 2:
                modeIndex = index;
                table = new OpenAddressingHashTable<string, int>();
                capacity = table.Capacity;
                logs.Append("Change Type of HashTable: OpenAddressing\n");
                break;
        }
        snapShot = table.GetSnapshot();
        logText.text = logs.ToString();
        VisualizeSlots();
    }

    private void OnProbeModeChanged(int index)
    {
        int count = slotContent.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            Destroy(slotContent.GetChild(i).gameObject);
        }

        switch (index)
        {
            case 0:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.Linear);
                logs.Append("Change Probing Strategy: Linear\n");
                break;
            case 1:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.Quadratic);
                logs.Append("Change Probing Strategy: Quadratic\n");
                break;
            case 2:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.DoubleHash);
                logs.Append("Change Probing Strategy: DoubleHash\n");
                break;
        }
        snapShot = table.GetSnapshot();
        logText.text = logs.ToString();
        VisualizeSlots();

    }

    private void SelectSlot(string key)
    {
        selectedKey = key;
        Debug.Log($"[Select] {key} 선택됨");
    }

    public void VisualizeSlots()
    {
        Debug.Log($"[VisualizeSlots] 호출됨 - 현재 childCount: {slotContent.childCount}, capacity: {snapShot.Capacity}");

        for (int i = 0; i < snapShot.Capacity; i++)
        {
            var slot = Instantiate(prefab, slotContent);
            var slotData = snapShot.Slots[i];

            slot.indexText.text = $"i : {i}";
            string slotKey = string.Empty;

            if (snapShot.TableType == "Chaining")
            {
                if (slotData.Chain != null && slotData.Chain.Count > 0)
                {
                    var parts = new StringBuilder();
                    foreach (var node in slotData.Chain)
                    {
                        slotKey = node.Key;
                        parts.Append($"K : {node.Key} V : {node.Value}\t");
                    }
                    slot.kvText.text = parts.ToString();
                    Debug.Log($"키-밸류: {snapShot.Slots[i].Key} - {snapShot.Slots[i].Value}");
                    slot.background.color = snapShot.Slots[i].State == SlotState.Occupied ? Color.green : Color.white;
                }
            }
            else
            {
                slotKey = slotData.Key;
                slot.kvText.text = snapShot.Slots[i].State == SlotState.Occupied ? $"K: {snapShot.Slots[i].Key}, V : {snapShot.Slots[i].Value} ": string.Empty;
                Debug.Log($"키-밸류: {snapShot.Slots[i].Key} - {snapShot.Slots[i].Value}");
                slot.background.color = snapShot.Slots[i].State == SlotState.Occupied ? Color.green : Color.white;
            } 

            slot.onClicked = () => SelectSlot(slotKey);
        }
    }

    private IEnumerator VisualizeNextFrame()
    {
        yield return null;
        VisualizeSlots();
    }

    private void OnAddSlot()
    {
        string key = keyInputField.text;
        int value;

        if (!int.TryParse(valueInputField.text, out value))
        {
            Debug.Log("[ADD] Value값 오류: 정수를 입력해주세요");
            return;
        }

        table[key] = value;

        logs.Append($"ADD: {key} - {value}\n");
        var conflict = false;
        switch (modeIndex)
        {
            case 0:
                conflict = ((SimpleHashTable<string, int>)table).isConflict;
                break;
            case 1:
                conflict = ((ChainingHashTable<string, int>)table).isConflict;
                break;
            case 2:
                conflict = ((OpenAddressingHashTable<string, int>)table).isConflict;
                break;
        }
        if (conflict)
            logs.Append($"Conflict: {key}\n");
        logText.text = logs.ToString();
        
        Debug.Log($"[Add] Debug: {key} - {value}");

        ClearSlot();
     
    }

    // --- 내부에서 삭제할 때 사용할 Clear 함수 ---
    private void ClearSlot()
    {
        snapShot = table.GetSnapshot();
        for (int i = slotContent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContent.GetChild(i).gameObject);
        }

        StartCoroutine(VisualizeNextFrame());
    }

    // --- 버튼에 연결된 Clear 함수 ---
    private void OnClearSlot()
    {
        switch (modeIndex)
        {
            case 0:
                table = new SimpleHashTable<string, int>();
                break;
            case 1:
                table = new ChainingHashTable<string, int>();
                break;
            case 2:
                table = new OpenAddressingHashTable<string, int>();
                break;
        }

        snapShot = table.GetSnapshot();
        for (int i = slotContent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContent.GetChild(i).gameObject);
        }

        logs.Append($"Clear: Clear All Slots");
        logText.text = logs.ToString();

        StartCoroutine(VisualizeNextFrame());
    }
    
    private void OnRemoveSlot()
    {
        if (string.IsNullOrEmpty(selectedKey))
        {
            Debug.Log("[Remove] 선택된 슬롯에 키가 없습니다.");
            return;
        }

        if (modeIndex == 1)
        {
            for (int i = 0; i < snapShot.Capacity; i++)
            {
                var slotData = snapShot.Slots[i];
                
                if (slotData.Chain != null && slotData.Chain.Count > 0)
                {
                    bool found = false;
                    var parts = new StringBuilder();
                    foreach (var node in slotData.Chain)
                    {
                        if (selectedKey == node.Key)
                        {
                            found = true;
                            parts.Replace($"K : {node.Key} V : {node.Value}\t", "");
                            
                        }
                    }
                    var targetSlot = slotContent.GetChild(i).GetComponent<UiHashSlot>();
                    targetSlot.kvText.text = parts.ToString();
                    targetSlot.background.color = parts.Length > 0 ? Color.green : Color.white;
                }
            }
        }
        
        table.Remove(selectedKey);
        logs.Append($"Remove: {selectedKey}");
        logText.text = logs.ToString();
        Debug.Log($"[Remove] 선택된 {selectedKey}가 삭제됐습니다.");
        
        selectedKey = null;

        ClearSlot();
    }
}
