using UnityEngine;

public class RedSwitchInteractable : MonoBehaviour, IInteractable
{
	[SerializeField] private string promptOff = "Switch on the light";
	[SerializeField] private string promptOn = "Switch off the light";

	[SerializeField] private AudioClip switchSFX;

	public void Interact(Player player)
	{
		RedLayerManager.Instance.ToggleLayer();

		if (switchSFX != null)
		{
			AudioManager.Instance.PlaySFX(switchSFX);
		}
	}

	public string GetPrompt()
	{
		return RedLayerManager.Instance.IsActive ? promptOn : promptOff;
	}
}