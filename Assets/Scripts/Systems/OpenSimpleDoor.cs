using UnityEngine;

public class OpenSimpleDoor : MonoBehaviour, IInteractable
{
	[SerializeField] private string doorID;
	[SerializeField] private string prompt = "Open door";
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private UnityEngine.Events.UnityEvent onOpened;

	[Header("Audio")]
	[SerializeField] private AudioSource unlockSFX;

	private bool _opened = false;

	private void Start()
	{
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag("opened_" + doorID))
		{
			_opened = true;
			if (doorAnimator != null)
			{
				doorAnimator.enabled = true;
				doorAnimator.Play(doorAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
			}
		}
	}

	public string GetPrompt()
	{
		return _opened ? "" : $"[E] {prompt}";
	}

	public void Interact(Player player)
	{
		if (_opened) return;
		_opened = true;
		unlockSFX?.Play();

		GameFlags.Instance?.SetFlag("opened_" + doorID);

		if (doorAnimator != null)
			doorAnimator.enabled = true;
		else
			Debug.LogWarning($"OpenSimpleDoor '{doorID}': no Animator assigned.");

		onOpened?.Invoke();
	}
}