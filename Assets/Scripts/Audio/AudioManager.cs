using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[Header("Audio Sources")]
	[SerializeField] private AudioSource ambientSource;
	[SerializeField] private AudioSource sfxSource;

	[Header("Settings")]
	[Range(0f, 1f)]
	[SerializeField] private float masterVolume = 1f;

	[SerializeField] private AudioClip defaultAmbient;

	private Coroutine fadeRoutine;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		if (ambientSource != null)
			ambientSource.loop = true;
	}

	// ------------------------
	// AMBIENT
	// ------------------------

	public void PlayAmbient(AudioClip clip, bool loop = true)
	{
		if (ambientSource == null) return;

		if (clip == null)
			clip = defaultAmbient;

		if (ambientSource.clip == clip)
			return;

		ambientSource.clip = clip;
		ambientSource.loop = loop;
		ambientSource.volume = masterVolume;
		ambientSource.Play();
	}

	public void StopAmbient()
	{
		if (ambientSource == null) return;

		ambientSource.Stop();
	}

	public void FadeAmbientTo(AudioClip newClip, float duration)
	{
		if (fadeRoutine != null)
			StopCoroutine(fadeRoutine);

		fadeRoutine = StartCoroutine(FadeRoutine(newClip, duration));
	}

	private IEnumerator FadeRoutine(AudioClip newClip, float duration)
	{
		if (ambientSource == null) yield break;

		float startVolume = ambientSource.volume;

		// Fade out
		float t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
			yield return null;
		}

		ambientSource.Stop();

		// Switch clip
		ambientSource.clip = newClip != null ? newClip : defaultAmbient;
		ambientSource.Play();

		// Fade in
		t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			ambientSource.volume = Mathf.Lerp(0f, masterVolume, t / duration);
			yield return null;
		}

		ambientSource.volume = masterVolume;
	}

	// ------------------------
	// SFX
	// ------------------------

	public void PlaySFX(AudioClip clip)
	{
		if (sfxSource == null || clip == null) return;

		sfxSource.PlayOneShot(clip, masterVolume);
	}

	// ------------------------
	// GLOBAL CONTROL
	// ------------------------

	public void SetMasterVolume(float volume)
	{
		masterVolume = Mathf.Clamp01(volume);

		if (ambientSource != null)
			ambientSource.volume = masterVolume;
	}
}