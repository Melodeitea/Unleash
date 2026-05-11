using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Talk";
    [SerializeField] private DialogueSequence sequence;

    [Header("Flag Gate")]
    [Tooltip("When true, the flag below must be active to allow interaction.")]
    [SerializeField] private bool requireFlag = false;
    [SerializeField] private string requiredFlag = "flashlight_on";
    [Tooltip("Prompt shown when the flag gate blocks interaction. Leave empty to show nothing.")]
    [SerializeField] private string lockedPrompt = "";

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceComplete;

    // ── IInteractable ────────────────────────────────────────

    public void Interact(Player player)
    {
        if (!IsFlagSatisfied()) return;

        if (sequence != null)
            DialogueManager.Instance.PlaySequence(sequence, OnComplete);
    }

    public string GetPrompt()
    {
        if (!IsFlagSatisfied())
            return lockedPrompt; // Empty string hides the prompt in most interaction UIs

        return prompt;
    }

    // ── Internals ────────────────────────────────────────────

    private bool IsFlagSatisfied()
    {
        if (!requireFlag) return true;
        if (GameFlags.Instance == null) return false;
        return GameFlags.Instance.GetFlag(requiredFlag);
    }

    private void OnComplete()
    {
        onSequenceComplete?.Invoke();
    }
}