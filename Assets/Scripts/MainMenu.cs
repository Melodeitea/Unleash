using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;

public class MainMenu : MonoBehaviour
{
    [Header("Scene names")]
    public string gameSceneName = "MainScene";
    public string creditsSceneName = "Credits";

    [Header("UI")]
    public Button loadButton;

    void Start()
    {
        // Lock or unlock Load button depending on save existence
        if (loadButton != null)
            loadButton.interactable = SaveSystem.HasSave();
    }

    public void StartGame()
    {
        // Ensure game is unpaused and cursor is locked for gameplay
        Time.timeScale = 1f;
        SetCursorLockedState(true);

        // Start a new game (does not try to load saved data)
        SceneManager.LoadScene(gameSceneName); 
    }

    public void LoadGame()
    {
        if (!SaveSystem.HasSave())
        {
            Debug.LogWarning("Load requested but no save file exists.");
            return;
        }

        // Ensure game is unpaused and cursor will be locked for gameplay
        Time.timeScale = 1f;
        SetCursorLockedState(true);

        // Register callback to apply loaded data once scene is loaded
        SceneManager.sceneLoaded += OnSceneLoadedApplySave;
        SceneManager.LoadScene(gameSceneName);
    }

    void OnSceneLoadedApplySave(Scene scene, LoadSceneMode mode)
    {
        // find player and call its LoadPlayer method
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.LoadPlayer();
        }
        else
        {
            Debug.LogWarning("Player not found in loaded scene to apply save.");
        }

        // Ensure cursor state and timescale are correct after scene load
        Time.timeScale = 1f;
        SetCursorLockedState(true);

        // Unregister callback
        SceneManager.sceneLoaded -= OnSceneLoadedApplySave;
    }

    public void OpenCredits()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Try to lock/hide cursor and set StarterAssets input cursorLocked if available
    void SetCursorLockedState(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

        // Try to set StarterAssetsInputs.cursorLocked if component exists
        // Use reflection to avoid compile error if StarterAssets namespace isn't present
        var starterInputs = FindObjectOfType<MonoBehaviour>();
        if (starterInputs != null)
        {
            // look for any component with type name "StarterAssetsInputs"
            var all = FindObjectsOfType<MonoBehaviour>();
            foreach (var comp in all)
            {
                var t = comp.GetType();
                if (t.Name == "StarterAssetsInputs" || t.FullName == "StarterAssets.StarterAssetsInputs")
                {
                    var field = t.GetField("cursorLocked", BindingFlags.Public | BindingFlags.Instance);
                    if (field != null && field.FieldType == typeof(bool))
                    {
                        field.SetValue(comp, locked);
                    }
                    else
                    {
                        var prop = t.GetProperty("cursorLocked", BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
                            prop.SetValue(comp, locked);
                    }
                    break;
                }
            }
        }
    }
}
