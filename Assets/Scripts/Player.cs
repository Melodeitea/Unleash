using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public int level;
	[SerializeField] private ItemDatabase itemDatabase; // drag your ItemDatabase asset here


	// ------------------------
	// SAVE
	// ------------------------
	public void SavePlayer()
	{
		SaveSystem.SavePlayer(this);
	}

	// ------------------------
	// LOAD
	// ------------------------
	public void LoadPlayer()
	{
		PlayerData data = SaveSystem.LoadPlayer();

		if (data == null)
		{
			Debug.LogWarning("No save to load.");
			return;
		}

		ApplyPlayerData(data);
	}

	// ------------------------
	// APPLY DATA
	// ------------------------

	private void ApplyPlayerData(PlayerData data)
	{
		// BASIC PLAYER
		level = data.level;
		if (data.position != null && data.position.Length >= 3)
			transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);

		// FLASHLIGHT
		var fl = FindObjectOfType<Flashlight>();
		if (fl != null)
		{
			fl.SetState(data.flashlightOn);
			if (data.flashlightEuler != null && data.flashlightEuler.Length >= 2)
				fl.SetRotationEuler(new Vector3(data.flashlightEuler[0], data.flashlightEuler[1], 0f));
		}

		// INVENTORY
		if (InventoryManager.Instance != null && itemDatabase != null)
		{
			InventoryManager.Instance.items.Clear();
			InventoryManager.Instance.files.Clear();
			InventoryManager.Instance.clues.Clear();

			if (data.inventoryItemIds != null)
			{
				itemDatabase.Init();
				foreach (string id in data.inventoryItemIds)
				{
					ItemData item = itemDatabase.Get(id);
					if (item != null)
						InventoryManager.Instance.AddItem(item);
					else
						Debug.LogWarning($"[Load] No ItemData found for id: '{id}' — did you fill in itemID on the asset?");
				}
			}
		}

	

		// ------------------------
		// GAME FLAGS
		// ------------------------
		if (GameFlags.Instance != null)
		{
			GameFlags.Instance.ClearAll();

			if (data.gameFlags != null)
			{
				foreach (string flag in data.gameFlags)
				{
					GameFlags.Instance.SetFlag(flag);
				}
			}
		}

		
	}
}