using UnityEngine;
using UnityEngine.SceneManagement;

public class DevSceneAdvanceKey : MonoBehaviour
{
	[Header("Input")]
	[SerializeField] private KeyCode advanceKey = KeyCode.F12;
	[SerializeField] private bool requireShift = true;

	[Header("Build Access")]
	[SerializeField] private bool allowInReleaseBuild = false;
	[SerializeField] private bool onlyInDevelopmentBuild = true;

	private void Update()
	{
		// ❌ Block if paused
		if (PauseMenu.IsPaused)
			return;

		// ❌ Build restriction logic fixed
		if (onlyInDevelopmentBuild)
		{
			if (!Debug.isDebugBuild && !allowInReleaseBuild)
				return;
		}

		// Shift requirement
		if (requireShift && !Input.GetKey(KeyCode.LeftShift))
			return;

		if (Input.GetKeyDown(advanceKey))
		{
			AdvanceScene();
		}
	}

	private void AdvanceScene()
	{
		int currentIndex = SceneManager.GetActiveScene().buildIndex;
		int totalScenes = SceneManager.sceneCountInBuildSettings;

		// Try Chapter system first
		if (ChapterManager.Instance != null)
		{
			var chapter = ChapterManager.Instance.CurrentChapter;

			if (chapter != null && !string.IsNullOrEmpty(chapter.nextSceneName))
			{
				Debug.Log("[DEV] Chapter advance → " + chapter.nextSceneName);
				SceneManager.LoadScene(chapter.nextSceneName);
				return;
			}
		}

		int nextIndex = currentIndex + 1;

		if (nextIndex >= totalScenes)
		{
			Debug.LogWarning("[DEV] Last scene reached.");
			return;
		}

		Debug.Log("[DEV] Build index advance → " + nextIndex);
		SceneManager.LoadScene(nextIndex);
	}
}