using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ChapterTransitionUI : MonoBehaviour
{
	public static ChapterTransitionUI Instance { get; private set; }

	[Header("UI")]
	[SerializeField] private Image fadePanel;         // full-screen black Image
	[SerializeField] private TextMeshProUGUI titleText;

	[Header("Timing")]
	[SerializeField] private float fadeDuration = 1f;
	[SerializeField] private float titleHoldDuration = 2.5f;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// Start invisible
		SetAlpha(0f);
		titleText.gameObject.SetActive(false);
	}

	public void StartTransition()
	{
		Debug.Log("[ChapterTransitionUI] Playing UI transition.");
		StartCoroutine(TransitionRoutine());
	}

	private IEnumerator TransitionRoutine()
	{
		var chapter = ChapterManager.Instance.CurrentChapter;

		// Fade to black
		yield return StartCoroutine(Fade(0f, 1f));

		// Show chapter title
		titleText.text = chapter.displayName;
		titleText.gameObject.SetActive(true);
		yield return new WaitForSeconds(titleHoldDuration);

		// Auto-save before advancing
		ChapterManager.Instance.AdvanceChapter();
		var player = FindObjectOfType<Player>();
		if (player != null) SaveSystem.SavePlayer(player);
		GameFlags.Instance.SaveFlags();

		// Load next scene
		SceneManager.LoadScene(chapter.nextSceneName);

		// Fade back in after scene loads
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		titleText.gameObject.SetActive(false);
		StartCoroutine(Fade(1f, 0f));
	}

	private IEnumerator Fade(float from, float to)
	{
		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			SetAlpha(Mathf.Lerp(from, to, t / fadeDuration));
			yield return null;
		}
		SetAlpha(to);
	}

	private void SetAlpha(float a)
	{
		var c = fadePanel.color;
		c.a = a;
		fadePanel.color = c;
	}
}