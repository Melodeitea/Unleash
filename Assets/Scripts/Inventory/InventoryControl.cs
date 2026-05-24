using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryControl : MonoBehaviour
{
	[Header("Screens")]
	[SerializeField] private GameObject inventoryScreen;
	[SerializeField] private GameObject inventoryFade;

	[Header("Tab Panels")]
	[SerializeField] private GameObject itemsPanel;
	[SerializeField] private GameObject notesPanel;
	[SerializeField] private GameObject cluesPanel;

	[Header("Tab Buttons (optional highlight)")]
	[SerializeField] private Image itemsTabImage;
	[SerializeField] private Image notesTabImage;
	[SerializeField] private Image cluesTabImage;
	[SerializeField] private Color tabActiveColor = Color.white;
	[SerializeField] private Color tabInactiveColor = new Color(1f, 1f, 1f, 0.4f);

	[Header("Audio")]
	[SerializeField] private AudioSource sfxOpen;
	[SerializeField] private AudioSource sfxClose;
	[SerializeField] private AudioSource sfxTabSwitch;
	[SerializeField] private AudioSource sfxItemSelect;

	[Header("UI")]
	[SerializeField] private InventoryUI inventoryUI;

	public bool isOpen { get; private set; } = false;
	private bool _canClose = false;
	private int _activeTab = 0;   // 0 = Items, 1 = Notes, 2 = Clues

	private const int TAB_COUNT = 3;

	// ── Input ─────────────────────────────────────────────────────

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (!isOpen && !_canClose) Open();
			else if (isOpen && _canClose) Close();
			return;
		}

		if (isOpen && _canClose)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				sfxTabSwitch?.Play();
				inventoryUI.NextTab();
			}
			else if (Input.GetKeyDown(KeyCode.Q))
			{
				sfxTabSwitch?.Play();
				inventoryUI.PrevTab();
			}
		}
	}

	// ── Open / Close ──────────────────────────────────────────────

	private void Open()
	{
		isOpen = true;
		sfxOpen?.Play();
		inventoryFade.SetActive(true);
		StartCoroutine(TransitionRoutine());
	}

	private void Close()
	{
		isOpen = false;
		_canClose = false;

		// Restore time before coroutine so WaitForSecondsRealtime isn't needed
		Time.timeScale = 1f;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		sfxClose?.Play();
		inventoryFade.SetActive(true);
		StartCoroutine(TransitionRoutine());
	}

	private IEnumerator TransitionRoutine()
	{
		yield return new WaitForSecondsRealtime(0.25f);

		if (isOpen)
		{
			inventoryScreen.SetActive(true);
			RefreshTabVisuals();
		}
		else
		{
			inventoryScreen.SetActive(false);
		}

		yield return new WaitForSecondsRealtime(0.25f);
		inventoryFade.SetActive(false);

		if (isOpen)
		{
			_canClose = true;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Time.timeScale = 0f;
		}
	}

	// ── Tab switching ─────────────────────────────────────────────

	private void SetTab(int index)
	{
		if (index == _activeTab) return;
		_activeTab = index;
		sfxTabSwitch?.Play();
		RefreshTabVisuals();
	}

	private void RefreshTabVisuals()
	{
		itemsPanel.SetActive(_activeTab == 0);
		notesPanel.SetActive(_activeTab == 1);
		cluesPanel.SetActive(_activeTab == 2);

		SetTabHighlight(itemsTabImage, _activeTab == 0);
		SetTabHighlight(notesTabImage, _activeTab == 1);
		SetTabHighlight(cluesTabImage, _activeTab == 2);
	}

	private void SetTabHighlight(Image img, bool active)
	{
		if (img == null) return;
		img.color = active ? tabActiveColor : tabInactiveColor;
	}

	// ── Called by item/note/clue buttons in the UI ────────────────

	public void OnEntrySelected()
	{
		sfxItemSelect?.Play();
	}

	// ── Public tab shortcuts (wirable from Inspector buttons) ─────

	public void GoToItems() => SetTab(0);
	public void GoToNotes() => SetTab(1);
	public void GoToClues() => SetTab(2);
}