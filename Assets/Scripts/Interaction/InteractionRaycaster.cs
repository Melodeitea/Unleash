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

	private IInteractable currentInteractable;

	private void Update()
	{
		if (justInteracted)
		{
			justInteracted = false;
			return;
		}
		Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

		if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
		{
			IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

			if (interactable != null)
			{
				currentInteractable = interactable;

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
					interactable.Interact(GetComponentInParent<Player>());

					HidePrompt();

					justInteracted = true;
					return;
				}

				return;
			}
		}

		currentInteractable = null;
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