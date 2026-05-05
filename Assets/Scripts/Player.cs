using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public int level;

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
		// ------------------------
		// BASIC PLAYER
		// ------------------------
		level = data.level;

		if (data.position != null && data.position.Length >= 3)
		{
			transform.position = new Vector3(
				data.position[0],
				data.position[1],
				data.position[2]
			);
		}

		// ------------------------
		// FLASHLIGHT
		// ------------------------
		var fl = FindObjectOfType<Flashlight>();
		if (fl != null)
		{
			fl.SetState(data.flashlightOn);

			if (data.flashlightEuler != null && data.flashlightEuler.Length >= 2)
			{
				fl.SetRotationEuler(new Vector3(
					data.flashlightEuler[0],
					data.flashlightEuler[1],
					0f
				));
			}
		}

		// ------------------------
		// INVENTORY
		// ------------------------
		//if (InventoryManager.Instance != null)
		//{
		//	InventoryManager.Instance.ClearAll();

		//	if (data.inventoryItemIds != null)
		//	{
		//		foreach (string id in data.inventoryItemIds)
		//		{
		//			InventoryItem item = FindInventoryItem(id);
		//			if (item != null)
		//			{
		//				InventoryManager.Instance.AddItem(item);
		//			}
		//			else
		//			{
		//				Debug.LogWarning($"[Load] Missing InventoryItem: {id}");
		//			}
		//		}
		//	}
		//}

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