using System.Collections;
using UnityEngine;
using TMPro;

public class MonologueManager : MonoBehaviour
{
	public static MonologueManager Instance { get; private set; }

	[SerializeField] private GameObject panel;
	[SerializeField] private TextMeshProUGUI monologueText;

	[SerializeField] private float autoDismissTime = 4f;
	[SerializeField] private KeyCode skipKey = KeyCode.E;

	private Coroutine currentRoutine;
	public bool IsPlaying { get; private set; }

	private void Awake()
	{
		Instance = this;
		panel.SetActive(false);
	}

	public void Play(string text)
	{
		if (currentRoutine != null)
			StopCoroutine(currentRoutine);

		currentRoutine = StartCoroutine(PlayRoutine(text));
	}

	private IEnumerator PlayRoutine(string text)
	{
		IsPlaying = true;

		panel.SetActive(true);
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

			if (Input.GetKeyDown(skipKey))
				break;

			yield return null;
		}

		Dismiss();
	}

	public void Dismiss()
	{
		panel.SetActive(false);
		IsPlaying = false;
	}

	public void PlayQueue(string[] texts)
	{
		StartCoroutine(QueueRoutine(texts));
	}

	private IEnumerator QueueRoutine(string[] texts)
	{
		IsPlaying = true;

		foreach (var text in texts)
		{
			panel.SetActive(true);
			monologueText.text = text;

			yield return new WaitUntil(() =>
				Input.GetKeyDown(skipKey) ||
				(autoDismissTime > 0 && Time.deltaTime >= autoDismissTime)
			);
		}

		Dismiss();
	}
}