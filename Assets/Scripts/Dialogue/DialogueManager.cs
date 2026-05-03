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
	[SerializeField] private KeyCode advanceKey = KeyCode.E;

	private DialogueSequence currentSequence;
	private int currentIndex;
	private Action onComplete;

	public bool IsPlaying { get; private set; }

	private void Awake()
	{
		Instance = this;
		dialoguePanel.SetActive(false);
	}

	private void Update()
	{
		if (!IsPlaying) return;

		if (Input.GetKeyDown(advanceKey))
		{
			AdvanceLine();
		}
	}

	public void PlaySequence(DialogueSequence seq, Action onCompleteCallback = null)
	{
		if (seq == null) return;

		if (seq.triggerOnce && seq.hasPlayed)
			return;

		currentSequence = seq;
		currentIndex = 0;
		onComplete = onCompleteCallback;

		dialoguePanel.SetActive(true);
		IsPlaying = true;

		ShowLine();

		seq.hasPlayed = true;
	}

	private void ShowLine()
	{
		if (currentSequence == null || currentSequence.lines.Length == 0)
			return;

		DialogueLine line = currentSequence.lines[currentIndex];

		speakerNameText.text = line.speakerName;
		lineText.text = line.line;
	}

	public void AdvanceLine()
	{
		currentIndex++;

		if (currentIndex >= currentSequence.lines.Length)
		{
			EndSequence();
		}
		else
		{
			ShowLine();
		}
	}

	private void EndSequence()
	{
		dialoguePanel.SetActive(false);
		IsPlaying = false;

		onComplete?.Invoke();
	}
}