using UnityEngine;
using UnityEngine.Events;

public class ExamineObject : MonoBehaviour, IInteractable
{
	[Header("Basic")]
	[SerializeField] private string prompt = "Examine";
	[TextArea]
	[SerializeField] private string monologueText;

	[Header("Rewards")]
	//[SerializeField] private InventoryItem itemToAdd;

	[Header("Behaviour")]
	[SerializeField] private bool examineOnce = false;
	[SerializeField] private bool requireRedLayer = false;

	[Header("Events")]
	[SerializeField] private UnityEvent onExamined;

	private bool hasBeenExamined = false;

	public void Interact(Player player)
	{
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
		return prompt;
	}
}