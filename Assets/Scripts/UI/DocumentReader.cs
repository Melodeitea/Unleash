using System.Text;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class DocumentReader : MonoBehaviour
{
	[Header("Data")]
	public DocumentContentSO content;
	public DocumentVisualSO visual;

	[Header("UI")]
	public GameObject readingUIRoot;
	public Transform visualContainer;
	public TextMeshProUGUI transcriptionText;

	[Header("Controls")]
	public KeyCode closeKey = KeyCode.E;
	public KeyCode toggleAsciiKey = KeyCode.Space;

	[Header("Optional")]
	public Behaviour[] componentsToDisable;

	bool _isViewing;
	bool _showingAscii;

	bool[] _previousComponentStates;
	GameObject _spawnedVisual;

	void Awake()
	{
		if (readingUIRoot != null)
			readingUIRoot.SetActive(false);

		if (componentsToDisable != null && componentsToDisable.Length > 0)
			_previousComponentStates = new bool[componentsToDisable.Length];
	}

	void Update()
	{
		if (!_isViewing) return;

		if (Input.GetKeyDown(closeKey))
			CloseDocument();

		if (Input.GetKeyDown(toggleAsciiKey))
			ToggleAsciiView();
	}

	// 🔥 CALLED BY ExamineObject
	public void OpenDocument()
	{
		if (readingUIRoot == null) return;

		_isViewing = true;
		_showingAscii = false;

		readingUIRoot.SetActive(true);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// TEXT
		if (content != null && transcriptionText != null)
			transcriptionText.text = content.transcriptionText;

		// VISUAL
		SpawnVisual();

		// Disable player
		if (componentsToDisable != null)
		{
			for (int i = 0; i < componentsToDisable.Length; i++)
			{
				if (componentsToDisable[i] == null) continue;

				_previousComponentStates[i] = componentsToDisable[i].enabled;
				componentsToDisable[i].enabled = false;
			}
		}
	}

	void CloseDocument()
	{
		_isViewing = false;
		_showingAscii = false;

		if (readingUIRoot != null)
			readingUIRoot.SetActive(false);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		if (_spawnedVisual != null)
			Destroy(_spawnedVisual);

		if (componentsToDisable != null)
		{
			for (int i = 0; i < componentsToDisable.Length; i++)
			{
				if (componentsToDisable[i] == null) continue;
				componentsToDisable[i].enabled = _previousComponentStates[i];
			}
		}
	}

	void SpawnVisual()
	{
		if (visual == null || visualContainer == null) return;

		// clean previous
		if (_spawnedVisual != null)
			Destroy(_spawnedVisual);

		if (visual.visualPrefab != null)
		{
			_spawnedVisual = Instantiate(visual.visualPrefab, visualContainer);
			_spawnedVisual.transform.localPosition = visual.localPositionOffset;
			_spawnedVisual.transform.localRotation = Quaternion.Euler(visual.localRotationOffset);
			_spawnedVisual.transform.localScale = visual.localScale;
		}
	}

	void ToggleAsciiView()
	{
		if (transcriptionText == null || content == null) return;

		_showingAscii = !_showingAscii;

		transcriptionText.text = _showingAscii
			? ConvertToNumeric(content.transcriptionText)
			: content.transcriptionText;
	}

	string ConvertToNumeric(string input)
	{
		if (string.IsNullOrEmpty(input)) return string.Empty;

		var sb = new StringBuilder(input.Length * 3);

		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];

			if (c == '\n' || c == '\r')
			{
				sb.AppendLine();
				continue;
			}

			sb.Append(c == ' ' ? "32" : ((int)c).ToString());

			if (i < input.Length - 1)
				sb.Append(' ');
		}

		return sb.ToString();
	}
}