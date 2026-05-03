using UnityEngine;
using System.Collections;

public class WiBSavePoint : MonoBehaviour, IInteractable
{
	[Header("Prompt")]
	[SerializeField] private string prompt = "Save";

	[Header("Visual")]
	[SerializeField] private Light wibLight;
	[SerializeField] private float pulseDuration = 1.5f;
	[SerializeField] private float maxIntensity = 5f;

	[Header("Audio")]
	[SerializeField] private AudioClip saveSFX;

	public void Interact(Player player)
	{
		if (player == null)
		{
			Debug.LogError("[WiBSavePoint] Player is null.");
			return;
		}

		SaveAll(player);

		if (wibLight != null)
			StartCoroutine(PulseRed());

		if (saveSFX != null)
			AudioManager.Instance.PlaySFX(saveSFX);
	}

	private void SaveAll(Player player)
	{
		// Save player data
		SaveSystem.SavePlayer(player.GetComponent<Player>());

		// Save global systems
		InventoryManager.Instance.SaveToPlayerPrefs();
		GameFlags.Instance.SaveFlags();

		Debug.Log("[WiBSavePoint] Game saved.");
	}

	private IEnumerator PulseRed()
	{
		float originalIntensity = wibLight.intensity;
		Color originalColor = wibLight.color;

		wibLight.color = Color.red;

		float half = pulseDuration * 0.5f;
		float t = 0f;

		// Fade up
		while (t < half)
		{
			t += Time.deltaTime;
			wibLight.intensity = Mathf.Lerp(0f, maxIntensity, t / half);
			yield return null;
		}

		t = 0f;

		// Fade down
		while (t < half)
		{
			t += Time.deltaTime;
			wibLight.intensity = Mathf.Lerp(maxIntensity, 0f, t / half);
			yield return null;
		}

		wibLight.intensity = originalIntensity;
		wibLight.color = originalColor;
	}

	public string GetPrompt()
	{
		return prompt;
	}
}