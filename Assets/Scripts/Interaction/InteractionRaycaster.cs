using UnityEngine;
using TMPro;

public class InteractionRaycaster : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float interactRange = 2.5f;
	[SerializeField] private KeyCode interactKey = KeyCode.E;

	[Header("UI")]
	[SerializeField] private GameObject promptUI;
	[SerializeField] private TextMeshProUGUI promptText;

	[SerializeField] private Transform rayOrigin;
	private bool justInteracted;
	private IInteractable lastInteracted;

	private IInteractable currentInteractable;

	private bool hasInteractedThisPress;

	private void Update()
	{
		
		Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

		if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
		{
			IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

			if (interactable != null)
			{
				currentInteractable = interactable;

				
				if (lastInteracted == currentInteractable)
				{
					HidePrompt();
					return;
				}

				string prompt = interactable.GetPrompt();

				if (!string.IsNullOrEmpty(prompt))
				{
					ShowPrompt(prompt);
				}
				else
				{
					HidePrompt();
				}

				if (Input.GetKeyDown(interactKey))
				{
					var player = GetComponentInParent<Player>();

					// 1. If monologue is already open → close it instead of interacting
					if (MonologueManager.Instance != null && MonologueManager.Instance.IsPlaying)
					{
						MonologueManager.Instance.Dismiss();
						HidePrompt();
						return;
					}

					// 2. Otherwise interact normally
					interactable.Interact(player);

					// 3. Show monologue AFTER interaction (if any)
					// NOTE: ExamineObject handles content, so we just ensure UI stays clean

					HidePrompt();
					lastInteracted = interactable;

					return;
				}

				return;
			}
		}
		currentInteractable = null;
		lastInteracted = null; // <-- ADD THIS
		HidePrompt();
	}

	private void ShowPrompt(string text)
	{
		if (!promptUI || !promptText) return;

		promptUI.SetActive(true);
		promptText.text = text;
		
	}

	private void HidePrompt()
	{
		if (!promptUI) return;

		promptUI.SetActive(false);
	}
}