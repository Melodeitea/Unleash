using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
	[Header("UI")]
	public GameObject pauseMenuUI;
	public GameObject firstSelected;

	[Header("Main menu")]
	public string mainMenuSceneName = "MainMenu";

	bool _isPaused;
	readonly List<Component> _componentsToToggle = new List<Component>();

	void Awake()
	{
		if (pauseMenuUI)
			pauseMenuUI.SetActive(false);

		// Try to find common player control components on object tagged "Player"
		var player = GameObject.FindWithTag("Player");
		if (player != null)
		{
			// Starter Assets controllers/inputs (namespaced types may or may not exist; we use GetComponent by type when present)
			AddIfFound(player, "StarterAssets.ThirdPersonController");
			AddIfFound(player, "StarterAssets.FirstPersonController");
			AddIfFound(player, "StarterAssetsInputs");
			AddIfFound(player, "CharacterController");
			// Add any custom control component names here if needed:
			// AddIfFound(player, "YourCustomPlayerController");
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePause();
		}
	}

	void TogglePause()
	{
		if (_isPaused) Resume();
		else Pause();
	}

	void Pause()
	{
		_isPaused = true;
		Time.timeScale = 0f;

		if (pauseMenuUI) pauseMenuUI.SetActive(true);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		SetComponentsEnabled(false);

		// set UI selection for keyboard/controller navigation
		if (firstSelected != null && EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(firstSelected);
		}
	}

	public void Resume()
	{
		_isPaused = false;
		Time.timeScale = 1f;

		if (pauseMenuUI) pauseMenuUI.SetActive(false);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		SetComponentsEnabled(true);

		if (EventSystem.current != null)
			EventSystem.current.SetSelectedGameObject(null);
	}

	public void SaveGame()
	{
		var player = FindObjectOfType<Player>();
		if (player != null)
			player.SavePlayer();
		// keep paused so user can continue or resume manually
	}

	public void LoadGame()
	{
		var player = FindObjectOfType<Player>();
		if (player != null)
			player.LoadPlayer();

		// resume after loading to return control to the player
		Resume();
	}

	public void OpenMainMenu()
	{
		Time.timeScale = 1f;
		if (!string.IsNullOrEmpty(mainMenuSceneName))
			SceneManager.LoadScene(mainMenuSceneName);
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}

	// --- Helper methods ---

	void AddIfFound(GameObject root, string typeName)
	{
		var comps = root.GetComponents<Component>();
		foreach (var c in comps)
		{
			if (c == null) continue;
			if (c.GetType().FullName == typeName || c.GetType().Name == typeName)
			{
				if (!_componentsToToggle.Contains(c))
					_componentsToToggle.Add(c);
			}
		}
	}

	void SetComponentsEnabled(bool enabled)
	{
		foreach (var comp in _componentsToToggle)
		{
			if (comp == null) continue;

			// most runtime components inherit Behaviour and have enabled property
			if (comp is Behaviour behaviour)
			{
				behaviour.enabled = enabled;
				continue;
			}

			// CharacterController and some legacy components also expose enabled
			var prop = comp.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
			if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
			{
				prop.SetValue(comp, enabled);
			}
		}
	}
}