using UnityEngine;
public class ChapterManager : MonoBehaviour
{
	public static ChapterManager Instance { get; private set; }
	[SerializeField] private ChapterData[] chapters;
	public int currentChapterIndex = 0;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
		Debug.Log($"[ChapterManager] Initialized. Current chapter index: {currentChapterIndex}");
	}

	public ChapterData CurrentChapter =>
		currentChapterIndex < chapters.Length ? chapters[currentChapterIndex] : null;

	public bool IsCurrentChapterComplete()
	{
		if (CurrentChapter == null)
		{
			Debug.LogWarning("[ChapterManager] CurrentChapter is null — is the chapters array filled in?");
			return false;
		}

		Debug.Log($"[ChapterManager] Checking flags for: {CurrentChapter.displayName}");

		foreach (string flag in CurrentChapter.requiredFlags)
		{
			bool set = GameFlags.Instance.GetFlag(flag);
			Debug.Log($"[ChapterManager] Flag '{flag}': {(set ? "SET" : "NOT SET")}");
			if (!set) return false;
		}

		Debug.Log("[ChapterManager] All flags checked — chapter is complete.");
		return true;
	}

	public void AdvanceChapter()
	{
		currentChapterIndex++;
		Debug.Log($"[ChapterManager] Advanced to chapter index {currentChapterIndex}.");
	}

	public void ResetToFirstChapter()
	{
		currentChapterIndex = 0;
	}
}