using UnityEngine;
using UnityEngine.Events;

public class ExamineObject : MonoBehaviour, IInteractable
{
	[Header("Basic")]
	[SerializeField] private string prompt = "Examine";
	[TextArea]
	[SerializeField] private string monologueText;

	[Header("Rewards")]
	[SerializeField] private InventoryItem itemToAdd;

	[Header("Behaviour")]
	[SerializeField] private bool examineOnce = false;
	[SerializeField] private bool requireRedLayer = false;

	[Header("Events")]
	[SerializeField] private UnityEvent onExamined;

	private bool hasBeenExamined = false;

	public void Interact(Player player)
	{
		if (examineOnce && hasBeenExamined)
			return;

		// Red layer restriction
		if (requireRedLayer && !RedLayerManager.Instance.IsActive)
			return;

		// Play monologue
		if (!string.IsNullOrEmpty(monologueText))
		{
			MonologueManager.Instance.Play(monologueText);
		}

		// Add item
		if (itemToAdd != null)
		{
			InventoryManager.Instance.AddItem(itemToAdd);
		}

		// Fire events
		onExamined?.Invoke();

		hasBeenExamined = true;

		// Disable interaction if needed
		if (examineOnce)
		{
			// Option 1: Disable this component
			enabled = false;

			// Option 2 (alternative): destroy component
			// Destroy(this);
		}
	}

	public string GetPrompt()
	{
		if (examineOnce && hasBeenExamined)
			return string.Empty;

		if (requireRedLayer && !RedLayerManager.Instance.IsActive)
			return string.Empty;

		return prompt;
	}
}