using System.Collections;
using UnityEngine;
using TMPro;

public class MonologueManager : MonoBehaviour
{
	public static MonologueManager Instance { get; private set; }

	[SerializeField] private GameObject panel;
	[SerializeField] private TextMeshProUGUI monologueText;

	[SerializeField] private float autoDismissTime = 4f;

	private Coroutine currentRoutine;
	public bool IsPlaying { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		panel.SetActive(false);
		IsPlaying = false;
	}

	public void Play(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;

		if (currentRoutine != null)
			StopCoroutine(currentRoutine);

		currentRoutine = StartCoroutine(PlayRoutine(text));

		if (!panel.activeSelf)
			panel.SetActive(true);
	}

	private IEnumerator PlayRoutine(string text)
	{
		IsPlaying = true;

		
		monologueText.text = text;

		float timer = 0f;

		while (true)
		{

			if (autoDismissTime > 0)
			{
				timer += Time.deltaTime;

				if (timer >= autoDismissTime)
					break;
			}

			yield return null;
		}

		Dismiss();
	}

	public void Dismiss()
	{
		panel.SetActive(false);
		IsPlaying = false;

		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
			currentRoutine = null;
		}
	}

	public void PlayQueue(string[] texts)
	{
		StartCoroutine(QueueRoutine(texts));
	}

	private IEnumerator QueueRoutine(string[] texts)
	{
		foreach (var text in texts)
		{
			panel.SetActive(true);
			monologueText.text = text;

			float timer = 0f;

			while (true)
			{
				timer += Time.deltaTime;

				if (timer >= autoDismissTime)
					break;

				yield return null;
			}
		}

		Dismiss();
	}
}