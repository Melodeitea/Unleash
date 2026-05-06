using UnityEngine;
using UnityEngine.Events;

public class CombinationLockTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string correctCode = "1234";
	[SerializeField] private int digitCount = 4;
	[SerializeField] private CombinationLockUI lockUI;
	[SerializeField] private Animator lockAnimator; // drag the door child object here
	[SerializeField] private UnityEvent onUnlocked;

	private bool _unlocked = false;
	private bool _isOpen = false;

	public string GetPrompt() => _unlocked ? "" : "[E] Open lock";

	public void Interact(Player player)
	{
		if (_unlocked || _isOpen) return;
		OpenLock();
	}

	private void OpenLock()
	{
		_isOpen = true;
		lockUI.Open(digitCount, OnCodeSubmitted, OnPlayerExit);
	}

	private void OnPlayerExit()
	{
		if (!_isOpen) return;
		_isOpen = false;
		lockUI.Close();
	}

	private void OnCodeSubmitted(string input)
	{
		if (input == correctCode)
			Unlock();
		else
			lockUI.ClearInput();
	}

	private void Unlock()
	{
		_unlocked = true;
		lockUI.LockInput();

		// ── Door animation ──────────────────────────────
		Debug.Log($"Unlock triggered. Animator assigned: {lockAnimator != null}");
		if (lockAnimator != null)
			lockAnimator.enabled = true;
		else
			Debug.LogWarning("lockAnimator is null — drag the door object into the Lock Animator slot on CombinationLockTarget.");

		onUnlocked?.Invoke();
		lockUI.PlaySuccessAndExit(OnPlayerExit);
	}
}