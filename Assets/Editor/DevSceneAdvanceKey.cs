using UnityEngine;
using UnityEngine.SceneManagement;

public class DevSceneAdvanceKey : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.F12;
    [SerializeField] private bool requireShift = true;

    [Header("Safety")]
    [SerializeField] private bool onlyInDevelopmentBuild = true;

    private void Update()
    {
        if (onlyInDevelopmentBuild && !Debug.isDebugBuild)
            return;

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

        // Try Chapter system first (preferred)
        if (ChapterManager.Instance != null)
        {
            var next = ChapterManager.Instance.CurrentChapter;

            if (next != null && !string.IsNullOrEmpty(next.nextSceneName))
            {
                Debug.Log("[DEV] Advancing via ChapterManager: " + next.nextSceneName);
                SceneManager.LoadScene(next.nextSceneName);
                return;
            }
        }

        // Fallback: build index order
        int nextIndex = currentIndex + 1;

        if (nextIndex >= totalScenes)
        {
            Debug.LogWarning("[DEV] Already at last scene in build order.");
            return;
        }

        string scenePath = SceneUtility.GetScenePathByBuildIndex(nextIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

        Debug.Log("[DEV] Advancing via Build Index → " + sceneName);

        SceneManager.LoadScene(nextIndex);
    }
}