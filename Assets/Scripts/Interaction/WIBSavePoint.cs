using UnityEngine;
using System.Collections;
using TMPro;

public class WiBSavePoint : MonoBehaviour, IInteractable
{
	[Header("Prompt")]
	[SerializeField] private string prompt = "Save";

	[Header("Visual")]
	[SerializeField] private Light wibLight;
	[SerializeField] private float pulseDuration = 1.5f;
	[SerializeField] private float maxIntensity = 5f;

	[Header("Audio")]
	[SerializeField] private AudioSource saveSFX;   // assign AudioSource with clip pre-loaded

	[Header("UI")]
	[SerializeField] private TextMeshProUGUI womanNameTMP;
	[SerializeField] private TextMeshProUGUI saveMessageTMP;

	// ── IInteractable ─────────────────────────────────────────────

	public string GetPrompt() => prompt;

	public void Interact(Player player)
	{
		if (player == null)
		{
			Debug.LogError("[WiBSavePoint] Player is null.");
			return;
		}

		SaveAll(player);
		saveSFX?.Play();                             // ← once, on successful save only

		if (womanNameTMP != null) womanNameTMP.text = "Woman in Black";
		if (saveMessageTMP != null) saveMessageTMP.text = "Sweet child, in time you'll see the light";
		if (wibLight != null) StartCoroutine(PulseRed());
	}

	// ── Save ──────────────────────────────────────────────────────

	private void SaveAll(Player player)
	{
		SaveSystem.SavePlayer(player);
		GameFlags.Instance.SaveFlags();
		Debug.Log("[WiBSavePoint] Game saved.");
	}

	// ── Light pulse ───────────────────────────────────────────────

	private IEnumerator PulseRed()
	{
		float originalIntensity = wibLight.intensity;
		Color originalColor = wibLight.color;

		wibLight.color = Color.red;
		float half = pulseDuration * 0.5f;
		float t = 0f;

		while (t < half)
		{
			t += Time.deltaTime;
			wibLight.intensity = Mathf.Lerp(0f, maxIntensity, t / half);
			yield return null;
		}

		t = 0f;
		while (t < half)
		{
			t += Time.deltaTime;
			wibLight.intensity = Mathf.Lerp(maxIntensity, 0f, t / half);
			yield return null;
		}

		wibLight.intensity = originalIntensity;
		wibLight.color = originalColor;
	}
}