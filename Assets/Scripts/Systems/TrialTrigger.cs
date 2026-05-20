using UnityEngine;

public class TrialTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "The floor is called to order.";

    public string GetPrompt() => prompt;

    public void Interact(Player player)
    {
        if (TrialManager.Instance != null)
            TrialManager.Instance.StartTrial();
    }
}