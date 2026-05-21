using UnityEngine;

public class ChapterTransitionZone : MonoBehaviour, IInteractable
{
    [SerializeField] private string readyPrompt = "Continue";
    [SerializeField] private string notReadyPrompt = "";

    private bool _triggered = false;

    public string GetPrompt()
    {
        if (_triggered) return string.Empty;
        return ChapterManager.Instance.IsCurrentChapterComplete()
            ? readyPrompt
            : notReadyPrompt;
    }

    public void Interact(Player player)
    {
        if (_triggered) return;
        if (!ChapterManager.Instance.IsCurrentChapterComplete()) return;

        _triggered = true;
        ChapterTransitionUI.Instance.StartTransition();
    }
}