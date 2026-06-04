using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	public static bool IsPaused { get; private set; }

	[Header("UI")]
	public GameObject pauseMenuUI;
	public GameObject firstSelected;

	[Header("Controls Panel")]
	[SerializeField] private GameObject bindingsPanel;

	[Header("Audio")]
	[SerializeField] private Slider masterVolumeSlider;

	[Header("Main Menu")]
	public string mainMenuSceneName = "MainMenu";

	private bool _isPaused;
	private readonly List<Behaviour> _componentsToToggle = new();

	private void Awake()
	{
		if (pauseMenuUI)
			pauseMenuUI.SetActive(false);

		if (bindingsPanel)
			bindingsPanel.SetActive(false);

		float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		AudioListener.volume = savedVolume;

		if (masterVolumeSlider != null)
		{
			masterVolumeSlider.value = savedVolume;
			masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
		}

		var player = GameObject.FindWithTag("Player");

		if (player != null)
		{
			AddIfFound(player, "StarterAssets.ThirdPersonController");
			AddIfFound(player, "StarterAssets.FirstPersonController");
			AddIfFound(player, "StarterAssetsInputs");
			AddIfFound(player, "CharacterController");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (CombinationLockUI.IsOpen)
				return;

			TogglePause();
		}
	}

	private void TogglePause()
	{
		if (_isPaused)
			Resume();
		else
			Pause();
	}

	private void Pause()
	{
		_isPaused = true;
		IsPaused = true;

		Time.timeScale = 0f;

		if (pauseMenuUI)
			pauseMenuUI.SetActive(true);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		SetComponentsEnabled(false);

		// Force clean UI selection (IMPORTANT for builds)
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(firstSelected);
		}
	}

	public void Resume()
	{
		_isPaused = false;
		IsPaused = false;

		Time.timeScale = 1f;

		if (pauseMenuUI)
			pauseMenuUI.SetActive(false);

		if (bindingsPanel)
			bindingsPanel.SetActive(false);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		SetComponentsEnabled(true);

		if (EventSystem.current != null)
			EventSystem.current.SetSelectedGameObject(null);
	}

	public void OpenControlsPanel()
	{
		if (bindingsPanel)
			bindingsPanel.SetActive(true);
	}

	public void CloseControlsPanel()
	{
		if (bindingsPanel)
			bindingsPanel.SetActive(false);
	}

	public void SetMasterVolume(float volume)
	{
		AudioListener.volume = volume;

		PlayerPrefs.SetFloat("MasterVolume", volume);
		PlayerPrefs.Save();
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

	private void AddIfFound(GameObject root, string typeName)
	{
		var comps = root.GetComponents<Component>();

		foreach (var c in comps)
		{
			if (c == null) continue;

			if (c.GetType().FullName == typeName ||
				c.GetType().Name == typeName)
			{
				if (c is Behaviour b && !_componentsToToggle.Contains(b))
					_componentsToToggle.Add(b);
			}
		}
	}

	private void SetComponentsEnabled(bool enabled)
	{
		foreach (var comp in _componentsToToggle)
		{
			if (comp == null) continue;

			comp.enabled = enabled;
		}
	}
}