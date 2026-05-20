using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance { get; private set; }

    [System.Serializable]
    public class Accusation
    {
        public string label;                          // editor label e.g. "The Ledger"
        [Header("Dialogue")]
        public DialogueSequence accusationSequence;   // workers accuse
        public DialogueSequence correctSequence;      // Roxanne presents evidence
        public DialogueSequence wrongSequence;        // Roxanne responds emotionally
        [Header("Evidence")]
        public ItemData correctItem;                  // the item that answers this accusation
        [HideInInspector] public bool answeredCorrectly;
    }

    [Header("Accusations")]
    [SerializeField] private Accusation[] accusations;

    [Header("Outcome")]
    [SerializeField] private string trialFlagID = "trial_5_1";
    [SerializeField] private UnityEvent onTrialComplete;   // fires regardless of outcome
    [SerializeField] private UnityEvent onAllCorrect;
    [SerializeField] private UnityEvent onAnyWrong;

    [Header("UI")]
    [SerializeField] private TrialItemSelector itemSelector;

    private int _currentIndex = 0;
    private bool _running = false;

    public bool TrialComplete { get; private set; }
    public bool AllCorrect { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public entry point ────────────────────────────────────────

    public void StartTrial()
    {
        if (_running || TrialComplete) return;
        _running = true;
        _currentIndex = 0;
        StartCoroutine(RunTrial());
    }

    // ── Trial loop ────────────────────────────────────────────────

    private IEnumerator RunTrial()
    {
        foreach (var accusation in accusations)
        {
            // 1. Play accusation dialogue
            bool dialogueDone = false;
            DialogueManager.Instance.PlaySequence(accusation.accusationSequence, () => dialogueDone = true);
            yield return new WaitUntil(() => dialogueDone);

            // 2. Open item selection UI
            ItemData chosen = null;
            bool selectionDone = false;

            itemSelector.Open(
                accusation.label,
                InventoryManager.Instance.items,
                (item) => { chosen = item; selectionDone = true; }
            );

            yield return new WaitUntil(() => selectionDone);

            // 3. Evaluate
            bool correct = chosen != null
                           && accusation.correctItem != null
                           && chosen.itemID == accusation.correctItem.itemID;

            accusation.answeredCorrectly = correct;

            // 4. Play response dialogue
            bool responseDone = false;
            DialogueSequence responseSeq = correct
                ? accusation.correctSequence
                : accusation.wrongSequence;

            if (responseSeq != null)
            {
                DialogueManager.Instance.PlaySequence(responseSeq, () => responseDone = true);
                yield return new WaitUntil(() => responseDone);
            }
        }

        EndTrial();
    }

    // ── Outcome ───────────────────────────────────────────────────

    private void EndTrial()
    {
        _running = false;
        TrialComplete = true;

        AllCorrect = true;
        foreach (var a in accusations)
            if (!a.answeredCorrectly) { AllCorrect = false; break; }

        // Write flags
        GameFlags.Instance?.SetFlag(trialFlagID + "_complete");
        if (AllCorrect)
            GameFlags.Instance?.SetFlag(trialFlagID + "_all_correct");

        onTrialComplete?.Invoke();
        if (AllCorrect) onAllCorrect?.Invoke();
        else onAnyWrong?.Invoke();

        Debug.Log($"[Trial] Complete. All correct: {AllCorrect}  " +
                  $"({CorrectCount()}/{accusations.Length})");
    }

    public int CorrectCount()
    {
        int n = 0;
        foreach (var a in accusations) if (a.answeredCorrectly) n++;
        return n;
    }

    public bool WasCorrect(int index) => accusations[index].answeredCorrectly;
}