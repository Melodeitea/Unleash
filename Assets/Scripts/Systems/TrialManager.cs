using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrialManager : MonoBehaviour
{
	public static TrialManager Instance { get; private set; }

	[System.Serializable]
	public class EvidenceVariant
	{
		[Tooltip("Flag that proves this accusation.")]
		public string flag;

		[Tooltip("Dialogue played if this flag exists.")]
		public DialogueSequence successSequence;
	}

	[System.Serializable]
	public class Accusation
	{
		public string label;

		[Header("Accusation")]
		public DialogueSequence accusationSequence;

		[Header("Valid Evidence Flags")]
		public List<EvidenceVariant> validEvidence = new();

		[Header("Failure")]
		public DialogueSequence wrongSequence;

		[HideInInspector]
		public bool answeredCorrectly;

		[HideInInspector]
		public string matchedFlag;
	}

	[Header("Accusations")]
	[SerializeField] private Accusation[] accusations;

	[Header("Outcome")]
	[SerializeField] private string trialFlagID = "trial_5_1";

	[SerializeField] private UnityEvent onTrialComplete;
	[SerializeField] private UnityEvent onAllCorrect;
	[SerializeField] private UnityEvent onAnyWrong;

	private bool _running;

	public bool TrialComplete { get; private set; }
	public bool AllCorrect { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	public void StartTrial()
	{
		if (_running || TrialComplete)
			return;

		_running = true;

		foreach (var accusation in accusations)
		{
			accusation.answeredCorrectly = false;
			accusation.matchedFlag = "";
		}

		StartCoroutine(RunTrial());
	}

	private IEnumerator RunTrial()
	{
		foreach (var accusation in accusations)
		{
			// Play accusation
			bool accusationDone = false;

			if (accusation.accusationSequence != null)
			{
				DialogueManager.Instance.PlaySequence(
					accusation.accusationSequence,
					() => accusationDone = true);

				yield return new WaitUntil(() => accusationDone);
			}

			// Check flags
			EvidenceVariant matchedEvidence = null;

			foreach (var evidence in accusation.validEvidence)
			{
				if (string.IsNullOrWhiteSpace(evidence.flag))
					continue;

				if (GameFlags.Instance != null &&
					GameFlags.Instance.GetFlag(evidence.flag))
				{
					matchedEvidence = evidence;
					accusation.matchedFlag = evidence.flag;
					break;
				}
			}

			accusation.answeredCorrectly = matchedEvidence != null;

			// Play result dialogue
			DialogueSequence resultSequence =
				matchedEvidence != null
				? matchedEvidence.successSequence
				: accusation.wrongSequence;

			if (resultSequence != null)
			{
				bool resultDone = false;

				DialogueManager.Instance.PlaySequence(
					resultSequence,
					() => resultDone = true);

				yield return new WaitUntil(() => resultDone);
			}
		}

		EndTrial();
	}

	private void EndTrial()
	{
		_running = false;
		TrialComplete = true;

		AllCorrect = true;

		foreach (var accusation in accusations)
		{
			if (!accusation.answeredCorrectly)
			{
				AllCorrect = false;
				break;
			}
		}

		if (GameFlags.Instance != null)
		{
			GameFlags.Instance.SetFlag(trialFlagID + "_complete");

			if (AllCorrect)
				GameFlags.Instance.SetFlag(trialFlagID + "_all_correct");
		}

		onTrialComplete?.Invoke();

		if (AllCorrect)
			onAllCorrect?.Invoke();
		else
			onAnyWrong?.Invoke();

		Debug.Log(
			$"[Trial] Complete. All correct: {AllCorrect} " +
			$"({CorrectCount()}/{accusations.Length})");
	}

	public int CorrectCount()
	{
		int count = 0;

		foreach (var accusation in accusations)
		{
			if (accusation.answeredCorrectly)
				count++;
		}

		return count;
	}

	public bool WasCorrect(int index)
	{
		if (index < 0 || index >= accusations.Length)
			return false;

		return accusations[index].answeredCorrectly;
	}

	public string GetMatchedFlag(int index)
	{
		if (index < 0 || index >= accusations.Length)
			return "";

		return accusations[index].matchedFlag;
	}
}