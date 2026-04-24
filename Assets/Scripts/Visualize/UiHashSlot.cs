using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UiHashSlot : MonoBehaviour
{
    public TextMeshProUGUI indexText;
    public TextMeshProUGUI kvText;
    public Image background;
    public Button slotButton;
    public Action onClicked;

    private void Awake()
    {
        slotButton.onClick.AddListener(() => onClicked?.Invoke());
    }
}
