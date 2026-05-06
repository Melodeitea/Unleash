using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueDetailUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private Button playButton;
	[SerializeField] private Sprite playSprite;
	[SerializeField] private Sprite pauseSprite;
	[SerializeField] private Image playButtonImage;

	private AudioClip currentClip;
	private AudioSource audioSource;
	private bool isPlaying;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (isPlaying && !audioSource.isPlaying)
		{
			isPlaying = false;
			playButtonImage.sprite = playSprite;
		}
	}
	public void ShowDetail(ItemData item)
	{
		titleText.text = item.itemName;
		descriptionText.text = item.description;

		currentClip = item.audioClip;

		audioSource.Stop();
		audioSource.clip = null;
		isPlaying = false;
		playButtonImage.sprite = playSprite;

		playButton.onClick.RemoveAllListeners();
		playButton.onClick.AddListener(ToggleAudio);
	}

	private void ToggleAudio()
	{
		if (currentClip == null) return;

		if (!isPlaying)
		{
			audioSource.clip = currentClip;
			audioSource.Play();
			isPlaying = true;
			playButtonImage.sprite = pauseSprite;
		}
		else
		{
			audioSource.Pause();
			isPlaying = false;
			playButtonImage.sprite = playSprite;
		}
	}

	public void HideDetail()
	{
		audioSource.Stop();
		audioSource.clip = null;
		isPlaying = false;
		playButtonImage.sprite = playSprite;
	}
}