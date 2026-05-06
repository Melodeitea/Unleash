using UnityEngine;
using UnityEngine.Events;

public class CombinationLockTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string correctCode = "1234";
	[SerializeField] private int digitCount = 4;
	[SerializeField] private CombinationLockUI lockUI;
	//[SerializeField] private Animator lockAnimator;         // the door/lock animator
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
		lockUI.LockInput();                          // disable further keypresses

		//if (lockAnimator != null)
		//	lockAnimator.SetTrigger("Open");         // play door/lock opening anim

		onUnlocked?.Invoke();
		lockUI.PlaySuccessAndExit(OnPlayerExit);     // wait, then auto-close
	}
}