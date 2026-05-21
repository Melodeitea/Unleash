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
    [SerializeField] private TextMeshProUGUI chapterNumberText;   // "Chapter 1"
    [SerializeField] private TextMeshProUGUI chapterTimeText;     // "2:00 AM"
    [SerializeField] private TextMeshProUGUI chapterNameText;     // "The Body"

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float titleHoldDuration = 2.5f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetAlpha(0f);
        SetTitlesVisible(false);
    }

    // ── Called by main menu "New Game" button ─────────────────────
    public void StartNewGame()
    {
        StartCoroutine(NewGameRoutine());
    }

    // ── Called by ChapterTransitionZone ──────────────────────────
    public void StartTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    // ── New game: fade out → load chapter 1 → intro ──────────────
    private IEnumerator NewGameRoutine()
    {
        yield return StartCoroutine(Fade(0f, 1f));

        // ChapterManager should already be initialised to Chapter 1
        string sceneName = ChapterManager.Instance.CurrentChapter.nextSceneName;
        SceneManager.LoadScene(sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ── Chapter-to-chapter: fade out → save → load → intro ───────
    private IEnumerator TransitionRoutine()
    {
        // Cache before advancing
        string nextScene = ChapterManager.Instance.CurrentChapter.nextSceneName;

        yield return StartCoroutine(Fade(0f, 1f));

        ChapterManager.Instance.AdvanceChapter();

        var player = FindObjectOfType<Player>();
        if (player != null) SaveSystem.SavePlayer(player);
        GameFlags.Instance.SaveFlags();

        SceneManager.LoadScene(nextScene);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ── Fires after every scene load ─────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(IntroRoutine());
    }

    // ── Show chapter titles then fade in ─────────────────────────
    private IEnumerator IntroRoutine()
    {
        var chapter = ChapterManager.Instance.CurrentChapter;

        chapterNumberText.text = $"Chapter {chapter.chapterNumber}";
        chapterTimeText.text = chapter.chapterTime;
        chapterNameText.text = chapter.displayName;

        SetTitlesVisible(true);
        yield return new WaitForSeconds(titleHoldDuration);
        SetTitlesVisible(false);

        yield return StartCoroutine(Fade(1f, 0f));
    }

    // ── Helpers ───────────────────────────────────────────────────
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
        var c = fadePanel.color; c.a = a; fadePanel.color = c;
    }

    private void SetTitlesVisible(bool visible)
    {
        chapterNumberText.gameObject.SetActive(visible);
        chapterTimeText.gameObject.SetActive(visible);
        chapterNameText.gameObject.SetActive(visible);
    }
}