using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections;

// 더미데이터 활용
public class UiHashTableTest : MonoBehaviour
{
    // --- 인스펙터 연결 필드 ---
    public UiHashSlot prefab;
    public Transform content;
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

    private void Awake()
    {
        selectedKey = string.Empty;
        table = new SimpleHashTable<string, int>();
        snapShot = table.GetSnapshot();
        
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
        int count = content.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        switch (index)
        {
            case 0:
                modeIndex = index;
                table = new SimpleHashTable<string, int>();
                break;
            case 1: 
                modeIndex = index;
                table = new ChainingHashTable<string, int>();
                break;
            case 2:
                modeIndex = index;
                table = new OpenAddressingHashTable<string, int>();
                break;
        }
        snapShot = table.GetSnapshot();
        VisualizeSlots();
    }

    private void OnProbeModeChanged(int index)
    {
        int count = content.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        switch (index)
        {
            case 0:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.Linear);
                break;
            case 1:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.Quadratic);
                break;
            case 2:
                table = new OpenAddressingHashTable<string, int>(ProbingStrategy.DoubleHash);
                break;
        }
        snapShot = table.GetSnapshot();
        VisualizeSlots();

    }

    private void SelectSlot(string key)
    {
        selectedKey = key;
        Debug.Log($"[Select] {key} 선택됨");
    }

    public void VisualizeSlots()
    {
        Debug.Log($"[VisualizeSlots] 호출됨 - 현재 childCount: {content.childCount}, capacity: {snapShot.Capacity}");

        for (int i = 0; i < snapShot.Capacity; i++)
        {
            var slot = Instantiate(prefab, content);
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

        Debug.Log($"[Add] Debug: {key} - {value}");

        ClearSlot();
     
    }

    // --- 내부에서 삭제할 때 사용할 Clear 함수 ---
    private void ClearSlot()
    {
        snapShot = table.GetSnapshot();
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
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
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

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
                    var parts = new StringBuilder();
                    foreach (var node in slotData.Chain)
                    {
                        if (selectedKey != node.Key)
                        {
                            parts.Append($"K : {node.Key} V : {node.Value}\t");
                            
                        }
                    }
                    var targetSlot = content.GetChild(i).GetComponent<UiHashSlot>();
                    targetSlot.kvText.text = parts.ToString();
                    targetSlot.background.color = parts.Length > 0 ? Color.green : Color.white;
                }
            }
        }
        else
        {
            table.Remove(selectedKey);
            Debug.Log($"[Remove] 선택된 {selectedKey}가 삭제됐습니다.");
        }
        selectedKey = null;


        ClearSlot();
    }
}
