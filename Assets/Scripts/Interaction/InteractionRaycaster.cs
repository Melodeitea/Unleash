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

	private IInteractable lastInteracted;
	private IInteractable nearestInteractable;

	private void Update()
	{
		// Don't update prompt or accept input while dialogue is open
		if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
		{
			HidePrompt();
			return;
		}

		nearestInteractable = GetNearestInteractable();

		if (nearestInteractable != null)
		{
			ShowPrompt(nearestInteractable.GetPrompt());

			if (Input.GetKeyDown(interactKey))
			{
				if (MonologueManager.Instance != null && MonologueManager.Instance.IsPlaying)
				{
					MonologueManager.Instance.Dismiss();
					HidePrompt();
					return;
				}

				var player = GetComponentInParent<Player>();
				nearestInteractable.Interact(player);
				HidePrompt();
			}
		}
		else
		{
			HidePrompt();
		}
	}

	private IInteractable GetNearestInteractable()
	{
		Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

		IInteractable nearest = null;
		float closestDistance = Mathf.Infinity;

		foreach (Collider col in hits)
		{
			IInteractable interactable = col.GetComponentInParent<IInteractable>();
			if (interactable == null) continue;

			float dist = Vector3.Distance(transform.position, col.transform.position);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				nearest = interactable;
			}
		}

		return nearest;
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, interactRange);
	}
}