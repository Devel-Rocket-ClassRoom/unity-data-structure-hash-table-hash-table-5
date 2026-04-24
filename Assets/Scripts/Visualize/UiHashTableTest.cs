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
    private HashTableSnapshot<string, int> snapShot = new DummySimple().GetSnapshot();
    private IHashTable<string, int> table;
    private int modeIndex; // clear후 다시 생성할 때 mode 기록용
    private string seletedKey;

    private void Awake()
    {
        dropdown.onValueChanged.AddListener(OnModeChanged);
        addButton.onClick.AddListener(OnAddSlot);
        clearButton.onClick.AddListener(OnClearSlot);
        removeButton.onClick.AddListner(OnRemoveSlot);

    }
    private void Start()
    {
        VisualizeSlots();
    }


    public void OnModeChanged(int index)
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
                snapShot = new DummySimple().GetSnapshot();
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
            string slotKey = slotData.Key;
            slot.onClicked = () => SelectSlot(slotKey);

            if (snapShot.TableType == "Chaining")
            {
                if (slotData.Chain != null && slotData.Chain.Count > 0)
                {
                    var parts = new StringBuilder();
                    foreach (var node in slotData.Chain)
                    {
                        parts.Append($"K : {node.Key} v : {node.Value}\t");
                    }
                    slot.kvText.text = parts.ToString();
                    Debug.Log($"키-밸류: {snapShot.Slots[i].Key} - {snapShot.Slots[i].Value}");
                    slot.background.color = snapShot.Slots[i].State == SlotState.Occupied ? Color.green : Color.white;
                }
            }
            else
            {
                slot.kvText.text = snapShot.Slots[i].State == SlotState.Occupied ? $"K: {snapShot.Slots[i].Key}, V : {snapShot.Slots[i].Value} ": string.Empty;
                Debug.Log($"키-밸류: {snapShot.Slots[i].Key} - {snapShot.Slots[i].Value}");
                slot.background.color = snapShot.Slots[i].State == SlotState.Occupied ? Color.green : Color.white;
            } 
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
                snapShot = new DummySimple().GetSnapshot();
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
        
    }
}
