using UnityEngine;
using TMPro;

public class InteractionRaycaster : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float interactRange = 2.5f;
	[SerializeField] private KeyCode interactKey = KeyCode.E;
	[SerializeField] private LayerMask interactableLayer;

	[Header("UI")]
	[SerializeField] private GameObject promptUI;
	[SerializeField] private TextMeshProUGUI promptText;

	private Camera playerCamera;
	private IInteractable currentInteractable;

	private void Awake()
	{
		playerCamera = Camera.main;
		HidePrompt();
	}


	private void Update()
	{

		if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
		{
			HidePrompt();
			return;
		}
		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
		{
			IInteractable interactable = hit.collider.GetComponent<IInteractable>();

			if (interactable != null)
			{
				currentInteractable = interactable;

				string prompt = interactable.GetPrompt();

				if (!string.IsNullOrEmpty(prompt))
				{
					ShowPrompt(prompt);

					if (Input.GetKeyDown(interactKey))
					{
						interactable.Interact(GetComponent<Player>());
					}
				}
				else
				{
					HidePrompt();
				}

				return;
			}
		}

		currentInteractable = null;
		HidePrompt();
	}

	private void ShowPrompt(string text)
	{
		if (promptUI == null || promptText == null) return;

		promptUI.SetActive(true);
		promptText.text = text;
	}

	private void HidePrompt()
	{
		if (promptUI == null) return;

		promptUI.SetActive(false);
	}
}