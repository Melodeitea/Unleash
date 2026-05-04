using UnityEngine;

public interface IInteractable
{
	public void Interact(Player player);
	string GetPrompt();
}