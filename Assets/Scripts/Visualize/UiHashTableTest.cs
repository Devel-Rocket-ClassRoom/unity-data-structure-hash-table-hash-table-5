using UnityEngine;
using TMPro;

// 더미데이터 활용
public class UiHashTableTest : MonoBehaviour
{
    public UiHashSlot prefab;
    public Transform content;
    public TMP_Dropdown dropdown;
    private HashTableSnapshot<string, int> slots = new DummySimple().GetSnapshot();

    private void Awake()
    {
        dropdown.onValueChanged.AddListener(OnModeChanged);
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
                slots = new DummySimple().GetSnapshot();
                break;
            case 1: 
                slots = new DummyChain().GetSnapshot();
                break;
            case 2:
                slots = new DummyOpenAddressing().GetSnapshot();
                break;
        }

        VisualizeSlots();
    }

    public void VisualizeSlots()
    {
        for (int i = 0; i < slots.Capacity; i++)
        {
            var slot = Instantiate(prefab, content);
            slot.indexText.text = $"i : {i}";
            slot.kvText.text = slots.Slots[i].State == SlotState.Occupied ? $"K: {slots.Slots[i].Key}, V : {slots.Slots[i].Value} ": string.Empty;
            slot.background.color = slots.Slots[i].State == SlotState.Occupied ? Color.green : Color.white;
        }
    }

}
