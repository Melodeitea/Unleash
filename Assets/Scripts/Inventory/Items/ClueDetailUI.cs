using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueDetailUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private Button playButton;

	private AudioClip currentClip;
	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void ShowDetail(ItemData item)
	{
		titleText.text = item.itemName;
		descriptionText.text = item.description;

		currentClip = item.audioClip;

		playButton.onClick.RemoveAllListeners();
		playButton.onClick.AddListener(PlayAudio);
	}

	private void PlayAudio()
	{
		if (currentClip == null) return;

		audioSource.Stop();
		audioSource.clip = currentClip;
		audioSource.Play();
	}
}