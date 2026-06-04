using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CompletionistCheck : MonoBehaviour
{
	[Header("Required Collectibles")]
	[Tooltip("Every clue ItemData the player must have collected.")]
	[SerializeField] private List<ItemData> requiredClues = new();

	[Tooltip("Every note ItemData the player must have collected.")]
	[SerializeField] private List<ItemData> requiredNotes = new();

	[Header("Achievement")]
	[SerializeField] private string achievementFlag = "achievement_completionist";
	[SerializeField] private UnityEvent onAchievementUnlocked;

	[Header("Achievement Notification")]
	[SerializeField] private GameObject achievementNotificationPanel;
	[SerializeField] private float notificationDuration = 4f;

	private void Start()
	{
		if (HasAllCollectibles())
			UnlockAchievement();
	}

	private IEnumerator ShowAchievementNotification()
	{
		if (achievementNotificationPanel == null)
			yield break;

		achievementNotificationPanel.SetActive(true);

		yield return new WaitForSeconds(notificationDuration);

		achievementNotificationPanel.SetActive(false);
	}

	private bool HasAllCollectibles()
	{
		if (GameFlags.Instance == null)
			return false;

		foreach (var clue in requiredClues)
		{
			if (clue == null) continue;

			if (!GameFlags.Instance.GetFlag("picked_up_" + clue.itemID))
				return false;
		}

		foreach (var note in requiredNotes)
		{
			if (note == null) continue;

			if (!GameFlags.Instance.GetFlag("picked_up_" + note.itemID))
				return false;
		}

		return true;
	}

	private void UnlockAchievement()
	{
		if (GameFlags.Instance == null)
			return;

		if (GameFlags.Instance.GetFlag(achievementFlag))
			return; // already unlocked

		GameFlags.Instance.SetFlag(achievementFlag);

		// ❌ REMOVED:
		// GameFlags.Instance.SaveFlags();

		Debug.Log("[CompletionistCheck] Achievement unlocked: Completionist");

		if (achievementNotificationPanel != null)
			StartCoroutine(ShowAchievementNotification());

		onAchievementUnlocked?.Invoke();
	}
}