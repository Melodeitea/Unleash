using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
	[Header("Tabs")]
	[SerializeField] private GameObject itemsPanel;
	[SerializeField] private GameObject filesPanel;
	[SerializeField] private GameObject cluesPanel;
	[SerializeField] private Button filesTabBtn;
	[SerializeField] private Button cluesTabBtn;

	[Header("Scroll List (shared)")]
	[SerializeField] private Transform listContent;       // ScrollView > Viewport > Content
	[SerializeField] private GameObject slotPrefab;

	[Header("Detail Panel — Items")]
	[SerializeField] private Image detailIcon;
	[SerializeField] private TextMeshProUGUI detailName;
	[SerializeField] private TextMeshProUGUI detailDesc;
	[SerializeField] private GameObject itemDetailSection;

	[Header("Detail Panel — Files")]
	[SerializeField] private TextMeshProUGUI fileBodyText;

	[Header("Detail Panel — Clues")]
	[SerializeField] private TextMeshProUGUI clueTitle;
	[SerializeField] private Button playAudioBtn;
	private AudioSource _audioSource;

	[Header("Back Buttons")]
	[SerializeField] private Button filesBackBtn;
	[SerializeField] private Button cluesBackBtn;

	private ItemData _selected;
	private ItemType _currentTab = ItemType.Item;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
	}

	private void OnEnable()
	{
		InventoryManager.Instance.OnInventoryChanged += Refresh;
		filesTabBtn.onClick.AddListener(() => SwitchTab(ItemType.File));
		cluesTabBtn.onClick.AddListener(() => SwitchTab(ItemType.Clue));
		playAudioBtn.onClick.AddListener(OnPlayClicked);
		SwitchTab(ItemType.Item);
		filesBackBtn.onClick.AddListener(() => SwitchTab(ItemType.Item));
		cluesBackBtn.onClick.AddListener(() => SwitchTab(ItemType.Item));
	}

	private void OnDisable()
	{
		if (InventoryManager.Instance != null)
			InventoryManager.Instance.OnInventoryChanged -= Refresh;

		filesTabBtn.onClick.RemoveAllListeners();
		cluesTabBtn.onClick.RemoveAllListeners();
		playAudioBtn.onClick.RemoveAllListeners();
		filesBackBtn.onClick.RemoveAllListeners();
		cluesBackBtn.onClick.RemoveAllListeners();
	}

	public void SwitchTab(ItemType tab)
	{
		_currentTab = tab;
		itemsPanel.SetActive(tab == ItemType.Item);
		filesPanel.SetActive(tab == ItemType.File);
		cluesPanel.SetActive(tab == ItemType.Clue);

		// Hide item detail when switching away
		if (tab != ItemType.Item)
		{
			itemDetailSection.SetActive(false);
			_selected = null;
		}


		Refresh();
	}

	private void Refresh()
	{
		// Clear old slots
		foreach (Transform child in listContent)
			Destroy(child.gameObject);

		List<ItemData> list = _currentTab switch
		{
			ItemType.Item => InventoryManager.Instance.items,
			ItemType.File => InventoryManager.Instance.files,
			ItemType.Clue => InventoryManager.Instance.clues,
			_ => InventoryManager.Instance.items
		};

		foreach (var item in list)
		{
			var go = Instantiate(slotPrefab, listContent);
			var slot = go.GetComponent<InventorySlot>();
			slot.Setup(item, OnSlotClicked);
		}

		// Reselect or clear detail
		if (_selected != null && list.Contains(_selected))
			ShowDetail(_selected);
		else
			ClearDetail();
	}

	private void OnSlotClicked(ItemData data)
	{
		_selected = data;
		ShowDetail(data);
	}

	public void ShowDetail(ItemData data)
	{
		switch (data.itemType)
		{
			case ItemType.Item:
				itemDetailSection.SetActive(true);
				detailIcon.sprite = data.icon;
				detailName.text = data.itemName;
				detailDesc.text = data.description;
				break;

			case ItemType.File:
				itemDetailSection.SetActive(false);
				fileBodyText.text = data.fileText;
				break;

			case ItemType.Clue:
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