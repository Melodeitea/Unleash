using UnityEngine;

public class Gramophone : MonoBehaviour, IInteractable
{
	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;

	[Header("Prompts")]
	[SerializeField] private string playPrompt = "Play record";
	[SerializeField] private string stopPrompt = "Stop record";

	[Header("Flags (optional)")]
	[SerializeField] private string playingFlag = "";   // leave empty to skip

	public string GetPrompt()
	{
		return audioSource != null && audioSource.isPlaying ? stopPrompt : playPrompt;
	}

	public void Interact(Player player)
	{
		if (audioSource == null) return;

		if (audioSource.isPlaying)
		{
			audioSource.Stop();
			if (!string.IsNullOrEmpty(playingFlag))
				GameFlags.Instance?.ClearFlag(playingFlag);
		}
		else
		{
			audioSource.Play();
			if (!string.IsNullOrEmpty(playingFlag))
				GameFlags.Instance?.SetFlag(playingFlag);
		}
	}
}