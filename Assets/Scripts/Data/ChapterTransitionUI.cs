using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ChapterTransitionUI : MonoBehaviour
{
	public static ChapterTransitionUI Instance { get; private set; }

	[Header("UI")]
	[SerializeField] private Image fadePanel;
	[SerializeField] private TextMeshProUGUI chapterNumberText;
	[SerializeField] private TextMeshProUGUI chapterTimeText;
	[SerializeField] private TextMeshProUGUI chapterNameText;

	[Header("Timing")]
	[SerializeField] private float fadeDuration = 1f;
	[SerializeField] private float titleHoldDuration = 2.5f;

	[Header("Audio")]
	[SerializeField] private AudioSource chapterSFX;

	[Header("Root")]
	[SerializeField] private GameObject transitionRoot;

	private bool isTransitioning;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;

		DontDestroyOnLoad(gameObject);

		SetAlpha(0f);
		SetTitlesVisible(false);

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	// ─────────────────────────────
	// NEW GAME
	// ─────────────────────────────
	public void StartNewGame()
	{
		if (isTransitioning) return;

		StartCoroutine(NewGameRoutine());
	}

	private IEnumerator NewGameRoutine()
	{
		isTransitioning = true;

		ChapterManager.Instance.ResetToFirstChapter();

		yield return Fade(0f, 1f);

		SceneManager.LoadScene(ChapterManager.Instance.CurrentChapter.nextSceneName);
	}

	// ─────────────────────────────
	// CHAPTER TRANSITION
	// ─────────────────────────────
	public void StartTransition()
	{
		if (isTransitioning) return;

		StartCoroutine(TransitionRoutine());
	}

	private IEnumerator TransitionRoutine()
	{
		isTransitioning = true;

		yield return Fade(0f, 1f);

		ChapterManager.Instance.AdvanceChapter();

		var player = FindFirstObjectByType<Player>();
		if (player != null)
			SaveSystem.SavePlayer(player);

		// ❌ REMOVED:
		// GameFlags.Instance.SaveFlags();

		SceneManager.LoadScene(
			ChapterManager.Instance.CurrentChapter.nextSceneName
		);
	}

	// ─────────────────────────────
	// SCENE LOADED → AUTO INTRO
	// ─────────────────────────────
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		StartCoroutine(IntroRoutine());
	}

	// ─────────────────────────────
	// INTRO SEQUENCE
	// ─────────────────────────────
	private IEnumerator IntroRoutine()
	{
		var chapter = ChapterManager.Instance.CurrentChapter;

		if (chapter == null)
		{
			isTransitioning = false;
			yield break;
		}

		if (transitionRoot != null)
			transitionRoot.SetActive(true);

		SetGameplayCursor(false);

		chapterNumberText.text = $"Chapter {chapter.chapterNumber}";
		chapterTimeText.text = chapter.chapterTime;
		chapterNameText.text = chapter.displayName;

		SetTitlesVisible(true);

		if (chapterSFX != null)
			chapterSFX.Play();

		yield return new WaitForSeconds(titleHoldDuration);

		SetTitlesVisible(false);

		if (chapterSFX != null)
			chapterSFX.Stop();

		yield return Fade(1f, 0f);

		if (transitionRoot != null)
		{
			foreach (Transform child in transitionRoot.transform)
			{
				child.gameObject.SetActive(false);
			}
		}

		SetGameplayCursor(true);

		isTransitioning = false;
	}

	// ─────────────────────────────
	// FADE
	// ─────────────────────────────
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

	private void SetGameplayCursor(bool gameplay)
	{
		Cursor.lockState = gameplay
			? CursorLockMode.Locked
			: CursorLockMode.None;

		Cursor.visible = !gameplay;
	}

	private void SetAlpha(float a)
	{
		var c = fadePanel.color;
		c.a = a;
		fadePanel.color = c;
	}

	private void SetTitlesVisible(bool visible)
	{
		chapterNumberText.gameObject.SetActive(visible);
		chapterTimeText.gameObject.SetActive(visible);
		chapterNameText.gameObject.SetActive(visible);
	}
}