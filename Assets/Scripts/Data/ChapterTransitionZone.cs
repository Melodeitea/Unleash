using UnityEngine;
public class ChapterTransitionZone : MonoBehaviour
{
	private bool _triggered = false;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"[TransitionZone] Something entered: {other.gameObject.name} (tag: {other.tag})");

		if (_triggered)
		{
			Debug.Log("[TransitionZone] Already triggered, ignoring.");
			return;
		}
		if (!other.CompareTag("Player"))
		{
			Debug.Log($"[TransitionZone] Not player, ignoring.");
			return;
		}

		Debug.Log("[TransitionZone] Player entered transition zone.");

		bool complete = ChapterManager.Instance.IsCurrentChapterComplete();
		Debug.Log($"[TransitionZone] Chapter complete: {complete}");

		if (!complete) return;

		_triggered = true;
		Debug.Log("[TransitionZone] Triggering chapter transition.");
		ChapterTransitionUI.Instance.StartTransition();
	}
}