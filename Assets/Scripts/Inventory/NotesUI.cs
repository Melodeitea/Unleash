using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotesUI : MonoBehaviour
{
	public static NotesUI Instance { get; private set; }

	[Header("Panel")]
	public GameObject panelRoot;

	[Header("Left — Note List")]
	public Transform noteListContainer;
	public GameObject noteEntryPrefab;  // simple button with title label

	[Header("Right — Document Reader")]
	public Transform visualContainer;
	public TextMeshProUGUI transcriptionText;
	public ScrollRect transcriptionScroll;

	[Header("Controls")]
	public KeyCode toggleKey = KeyCode.N;
	public KeyCode closeKey = KeyCode.Escape;

	[Header("Player lockout")]
	public Behaviour[] componentsToDisable;

	bool _isOpen;
	bool[] _prevStates;
	GameObject _spawnedVisual;

	void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(gameObject); return; }
		Instance = this;
		panelRoot.SetActive(false);
		if (componentsToDisable != null)
			_prevStates = new bool[componentsToDisable.Length];
	}

	void Start()
	{
		NotesManager.Instance.OnNotesChanged.AddListener(RefreshList);
	}

	void Update()
	{
		if (Input.GetKeyDown(toggleKey)) Toggle();
		if (_isOpen && Input.GetKeyDown(closeKey)) Close();
	}

	// Called directly by ExamineObject when a note is picked up
	public void Open(InventoryItem note)
	{
		panelRoot.SetActive(true);
		_isOpen = true;
		RefreshList();
		ShowNote(note);
		LockPlayer(true);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void Toggle()
	{
		if (_isOpen) Close();
		else
		{
			panelRoot.SetActive(true);
			_isOpen = true;
			RefreshList();
			// show first note if any
			var all = NotesManager.Instance.GetAll();
			if (all.Count > 0) ShowNote(all[0]);
			LockPlayer(true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void Close()
	{
		_isOpen = false;
		panelRoot.SetActive(false);
		DestroyVisual();
		LockPlayer(false);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	// ── List ──────────────────────────────────────────────
	void RefreshList()
	{
		foreach (Transform child in noteListContainer)
			Destroy(child.gameObject);

		foreach (InventoryItem note in NotesManager.Instance.GetAll())
		{
			InventoryItem captured = note; // closure capture
			GameObject entry = Instantiate(noteEntryPrefab, noteListContainer);
			entry.GetComponentInChildren<TextMeshProUGUI>().text = note.displayName;
			entry.GetComponent<Button>().onClick.AddListener(() => ShowNote(captured));
		}
	}

	// ── Reader ────────────────────────────────────────────
	void ShowNote(InventoryItem note)
	{
		// Text
		if (transcriptionText != null)
		{
			transcriptionText.text = note.transcriptionText;
			// reset scroll to top
			Canvas.ForceUpdateCanvases();
			transcriptionScroll.verticalNormalizedPosition = 1f;
		}

		// Visual
		DestroyVisual();
		if (note.visualPrefab != null && visualContainer != null)
		{
			_spawnedVisual = Instantiate(note.visualPrefab, visualContainer);
			_spawnedVisual.transform.localPosition = note.visualPositionOffset;
			_spawnedVisual.transform.localRotation = Quaternion.Euler(note.visualRotationOffset);
			_spawnedVisual.transform.localScale = note.visualScale;
		}
	}

	void DestroyVisual()
	{
		if (_spawnedVisual != null) { Destroy(_spawnedVisual); _spawnedVisual = null; }
	}

	// ── Player ────────────────────────────────────────────
	void LockPlayer(bool lockIt)
	{
		if (componentsToDisable == null) return;
		for (int i = 0; i < componentsToDisable.Length; i++)
		{
			if (componentsToDisable[i] == null) continue;
			if (lockIt)
			{
				_prevStates[i] = componentsToDisable[i].enabled;
				componentsToDisable[i].enabled = false;
			}
			else
			{
				componentsToDisable[i].enabled = _prevStates[i];
			}
		}
	}
}