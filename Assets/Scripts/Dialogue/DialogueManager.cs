using System;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager Instance { get; private set; }

	[Header("UI")]
	[SerializeField] private GameObject dialoguePanel;
	[SerializeField] private TextMeshProUGUI speakerNameText;
	[SerializeField] private TextMeshProUGUI lineText;

	[Header("Controls")]
	[SerializeField] private KeyCode advanceKey = KeyCode.Space;

	private DialogueSequence currentSequence;
	private int currentIndex;
	private Action onComplete;
	private bool justOpened;

	public bool IsPlaying { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		dialoguePanel.SetActive(false);
	}

	private void Update()
	{
		if (!IsPlaying) return;

		if (justOpened)
		{
			justOpened = false;
			return;
		}

		if (Input.GetKeyDown(advanceKey))
			AdvanceLine();
	}

	public void PlaySequence(DialogueSequence seq, Action onCompleteCallback = null)
	{
		if (seq == null) return;

		// 🔥 Prevent replay if triggerOnce already used
		if (seq.triggerOnce && seq.hasPlayed)
			return;

		currentSequence = seq;
		currentIndex = 0;
		onComplete = onCompleteCallback;

		justOpened = true;

		dialoguePanel.SetActive(true);
		IsPlaying = true;

		ShowLine();

		// Mark as played immediately
		seq.hasPlayed = true;
	}

	private void ShowLine()
	{
		if (currentSequence == null || currentSequence.lines.Length == 0) return;

		DialogueLine line = currentSequence.lines[currentIndex];

		speakerNameText.text = line.speakerName;
		lineText.text = line.line;
	}

	public void AdvanceLine()
	{
		currentIndex++;

		if (currentIndex >= currentSequence.lines.Length)
			EndSequence();
		else
			ShowLine();
	}

	private void EndSequence()
	{
		dialoguePanel.SetActive(false);
		IsPlaying = false;

		onComplete?.Invoke();
	}
}