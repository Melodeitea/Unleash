using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
	// ── Tab button rows — one per panel ──────────────────────────
	[System.Serializable]
	public struct TabButtonRow
	{
		public Button toItems;
		public Button toNotes;
		public Button toClues;
	}

	[Header("Panels")]
	[SerializeField] private GameObject itemsPanel;
	[SerializeField] private GameObject notesPanel;
	[SerializeField] private GameObject cluesPanel;

	[Header("Tab Buttons — one row per panel")]
	[SerializeField] private TabButtonRow itemsPanelTabs;
	[SerializeField] private TabButtonRow notesPanelTabs;
	[SerializeField] private TabButtonRow cluesPanelTabs;

	[Header("Tab Highlight Colors")]
	[SerializeField] private Color tabActiveColor = Color.white;
	[SerializeField] private Color tabInactiveColor = new Color(1f, 1f, 1f, 0.4f);

	[Header("Scroll List (shared — Items tab)")]
	[SerializeField] private Transform listContent;
	[SerializeField] private GameObject slotPrefab;

	[Header("Detail Panel — Items")]
	[SerializeField] private GameObject itemDetailSection;
	[SerializeField] private Image detailIcon;
	[SerializeField] private TextMeshProUGUI detailName;
	[SerializeField] private TextMeshProUGUI detailDesc;

	[Header("Detail Panel — Notes")]
	[SerializeField] private TextMeshProUGUI fileBodyText;

	[Header("Detail Panel — Clues")]
	[SerializeField] private TextMeshProUGUI clueTitle;
	[SerializeField] private Button playAudioBtn;

	// ── Tab order ─────────────────────────────────────────────────
	private static readonly ItemType[] TAB_ORDER =
		{ ItemType.Items, ItemType.Notes, ItemType.Clues };

	private ItemData _selected;
	private ItemType _currentTab = ItemType.Items;
	private AudioSource _audioSource;

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
	}

	private void OnEnable()
	{
		InventoryManager.Instance.OnInventoryChanged += Refresh;
		WireTabRow(itemsPanelTabs);
		WireTabRow(notesPanelTabs);
		WireTabRow(cluesPanelTabs);
		playAudioBtn.onClick.AddListener(OnPlayClicked);
		SwitchTab(_currentTab);
	}

	private void OnDisable()
	{
		if (InventoryManager.Instance != null)
			InventoryManager.Instance.OnInventoryChanged -= Refresh;

		UnwireTabRow(itemsPanelTabs);
		UnwireTabRow(notesPanelTabs);
		UnwireTabRow(cluesPanelTabs);
		playAudioBtn.onClick.RemoveAllListeners();
	}

	// ── Tab wiring ────────────────────────────────────────────────

	private void WireTabRow(TabButtonRow row)
	{
		row.toItems?.onClick.AddListener(() => SwitchTab(ItemType.Items));
		row.toNotes?.onClick.AddListener(() => SwitchTab(ItemType.Notes));
		row.toClues?.onClick.AddListener(() => SwitchTab(ItemType.Clues));
	}

	private void UnwireTabRow(TabButtonRow row)
	{
		row.toItems?.onClick.RemoveAllListeners();
		row.toNotes?.onClick.RemoveAllListeners();
		row.toClues?.onClick.RemoveAllListeners();
	}

	// ── Tab navigation (called by InventoryControl A / E) ────────

	public void NextTab()
	{
		int i = System.Array.IndexOf(TAB_ORDER, _currentTab);
		SwitchTab(TAB_ORDER[(i + 1) % TAB_ORDER.Length]);
	}

	public void PrevTab()
	{
		int i = System.Array.IndexOf(TAB_ORDER, _currentTab);
		SwitchTab(TAB_ORDER[(i - 1 + TAB_ORDER.Length) % TAB_ORDER.Length]);
	}

	// ── SwitchTab ─────────────────────────────────────────────────

	public void SwitchTab(ItemType tab)
	{
		_currentTab = tab;

		itemsPanel.SetActive(tab == ItemType.Items);
		notesPanel.SetActive(tab == ItemType.Notes);
		cluesPanel.SetActive(tab == ItemType.Clues);

		RefreshTabHighlights();

		if (tab != ItemType.Items)
		{
			itemDetailSection.SetActive(false);
			_selected = null;
		}

		Refresh();
	}

	// ── Tab button highlights ─────────────────────────────────────

	private void RefreshTabHighlights()
	{
		HighlightRow(itemsPanelTabs);
		HighlightRow(notesPanelTabs);
		HighlightRow(cluesPanelTabs);
	}

	private void HighlightRow(TabButtonRow row)
	{
		SetButtonHighlight(row.toItems, _currentTab == ItemType.Items);
		SetButtonHighlight(row.toNotes, _currentTab == ItemType.Notes);
		SetButtonHighlight(row.toClues, _currentTab == ItemType.Clues);
	}

	private void SetButtonHighlight(Button btn, bool active)
	{
		if (btn == null) return;
		var img = btn.GetComponent<Image>();
		if (img != null) img.color = active ? tabActiveColor : tabInactiveColor;
	}

	// ── Content refresh ───────────────────────────────────────────

	private void Refresh()
	{
		if (_currentTab != ItemType.Items) return;

		foreach (Transform child in listContent)
			Destroy(child.gameObject);

		foreach (var item in InventoryManager.Instance.items)
		{
			var go = Instantiate(slotPrefab, listContent);
			go.GetComponent<InventorySlot>().Setup(item, OnSlotClicked);
		}

		if (_selected != null && InventoryManager.Instance.items.Contains(_selected))
			ShowDetail(_selected);
		else
			ClearDetail();
	}

	// ── Selection ─────────────────────────────────────────────────

	private void OnSlotClicked(ItemData data)
	{
		_selected = data;
		ShowDetail(data);
	}

	public void ShowDetail(ItemData data)
	{
		switch (data.itemType)
		{
			case ItemType.Items:
				itemDetailSection.SetActive(true);
				detailIcon.sprite = data.icon;
				detailName.text = data.itemName;
				detailDesc.text = data.description;
				break;

			case ItemType.Notes:
				itemDetailSection.SetActive(false);
				fileBodyText.text = data.fileText;
				break;

			case ItemType.Clues:
				itemDetailSection.SetActive(false);
				clueTitle.text = data.itemName;
				playAudioBtn.gameObject.SetActive(data.audioClip != null);
				break;
		}
	}

	public void ClearDetail()
	{
		_selected = null;
		itemDetailSection.SetActive(false);
		detailName.text = "";
		detailDesc.text = "";
		fileBodyText.text = "";
	}

	private void OnPlayClicked()
	{
		if (_selected?.audioClip == null) return;
		_audioSource.clip = _selected.audioClip;
		_audioSource.Play();
	}
}