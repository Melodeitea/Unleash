using UnityEngine;
using UnityEngine.Events;

public class CombinationLockTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string correctCode = "1234";
	[SerializeField] private int digitCount = 4;
	[SerializeField] private CombinationLockUI lockUI;
	[SerializeField] private Animator lockAnimator;   // lock mechanism anim
	[SerializeField] private Animator objectAnimator; // door/drawer anim
	[SerializeField] private UnityEvent onUnlocked;
	[SerializeField] private string puzzleID;

	private bool _unlocked = false;
	private bool _isOpen = false;

	private void Start()
	{
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag("unlocked_" + puzzleID))
		{
			_unlocked = true;
			SnapToEnd(lockAnimator);
			SnapToEnd(objectAnimator);
		}
		else
		{
			if (lockAnimator != null) lockAnimator.enabled = false;
			if (objectAnimator != null) objectAnimator.enabled = false;
		}
	}

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
		if (input == correctCode) Unlock();
		else lockUI.ClearInput();
	}

	private void Unlock()
	{
		_unlocked = true;
		GameFlags.Instance?.SetFlag("unlocked_" + puzzleID);
		lockUI.LockInput();
		onUnlocked?.Invoke();
		lockUI.PlaySuccessAndExit(OnPlayerExit);
		StartCoroutine(PlaySequence());
	}

	private System.Collections.IEnumerator PlaySequence()
	{
		// 1. Play lock anim and wait for it
		if (lockAnimator != null)
		{
			lockAnimator.enabled = true;
			yield return WaitForAnim(lockAnimator);
		}

		// 2. Then play door/drawer anim
		if (objectAnimator != null)
			objectAnimator.enabled = true;
		else
			Debug.LogWarning($"CombinationLockTarget '{puzzleID}': no object Animator assigned.");
	}

	private System.Collections.IEnumerator WaitForAnim(Animator anim)
	{
		yield return null;
		float length = anim.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(length);
	}

	private void SnapToEnd(Animator anim)
	{
		if (anim == null) return;
		anim.enabled = true;
		anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
	}
}