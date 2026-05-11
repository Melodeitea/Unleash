using UnityEngine;
using UnityEngine.Events;

public class ExamineObject : MonoBehaviour, IInteractable
{
    [Header("Basic")]
    [SerializeField] private string prompt = "Examine";
    [TextArea]
    [SerializeField] private string monologueText;

    [Header("Flag Gate")]
    [Tooltip("When true, the flag below must be active to allow interaction.")]
    [SerializeField] private bool requireFlag = false;
    [SerializeField] private string requiredFlag = "flashlight_on";
    [Tooltip("Prompt shown when the flag gate blocks interaction. Leave empty to show nothing.")]
    [SerializeField] private string lockedPrompt = "";

    [Header("Behaviour")]
    [SerializeField] private bool examineOnce = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onExamined;

    private bool hasBeenExamined = false;

    // ── IInteractable ────────────────────────────────────────

    public void Interact(Player player)
    {
        if (!IsFlagSatisfied()) return;
        if (examineOnce && hasBeenExamined) return;

        if (!string.IsNullOrEmpty(monologueText))
            MonologueManager.Instance.Play(monologueText);

        onExamined?.Invoke();
        hasBeenExamined = true;

        if (examineOnce) enabled = false;
    }

    public string GetPrompt()
    {
        if (examineOnce && hasBeenExamined) return string.Empty;
        if (!IsFlagSatisfied()) return lockedPrompt;
        return prompt;
    }

    // ── Internals ────────────────────────────────────────────

    private bool IsFlagSatisfied()
    {
        if (!requireFlag) return true;
        if (GameFlags.Instance == null) return false;
        return GameFlags.Instance.GetFlag(requiredFlag);
    }
}