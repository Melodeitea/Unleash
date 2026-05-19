using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrialItemSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject itemButtonPrefab;  // Button + TMP label
    [SerializeField] private Button skipButton;        // "Respond without evidence"
    [SerializeField] private TextMeshProUGUI accusationLabel;

    private System.Action<ItemData> _onSelected;

    // ── Open / Close ──────────────────────────────────────────────

    public void Open(string label, List<ItemData> items, System.Action<ItemData> onSelected)
    {
        _onSelected = onSelected;
        panel.SetActive(true);

        if (accusationLabel != null)
            accusationLabel.text = label;

        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Spawn one button per inventory item
        foreach (var item in items)
        {
            var btn = Instantiate(itemButtonPrefab, buttonContainer);
            var label_text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label_text != null) label_text.text = item.itemName;

            var captured = item;
            btn.GetComponent<Button>().onClick.AddListener(() => Select(captured));
        }

        // Skip — respond without evidence
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() => Select(null));
    }

    private void Close()
    {
        panel.SetActive(false);
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
    }

    private void Select(ItemData item)
    {
        Close();
        _onSelected?.Invoke(item);  // null = no evidence presented
    }
}