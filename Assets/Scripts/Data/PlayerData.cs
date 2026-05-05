using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
	// ------------------------
	// PLAYER STATE
	// ------------------------
	public int level;
	public float[] position;

	// ------------------------
	// FLASHLIGHT
	// ------------------------
	public bool flashlightOn;
	public float[] flashlightEuler;

	// ------------------------
	// INVENTORY
	// ------------------------
	public List<string> inventoryItemIds;

	// ------------------------
	// GAME FLAGS (progression)
	// ------------------------
	public List<string> gameFlags;

	// ------------------------
	// RED LAYER
	// ------------------------
	public bool redLayerActive;

	public PlayerData(Player player)
	{
		// ------------------------
		// BASIC PLAYER
		// ------------------------
		level = player.level;

		position = new float[3];
		position[0] = player.transform.position.x;
		position[1] = player.transform.position.y;
		position[2] = player.transform.position.z;

		// ------------------------
		// FLASHLIGHT
		// ------------------------
		var fl = player.GetComponentInChildren<Flashlight>();
		if (fl != null)
		{
			flashlightOn = fl.IsOn;

			var rot = fl.GetRotationEuler();
			flashlightEuler = new float[2];
			flashlightEuler[0] = rot.x;
			flashlightEuler[1] = rot.y;
		}
		else
		{
			flashlightOn = false;
			flashlightEuler = new float[2] { 0f, 0f };
		}

		// ------------------------
		// INVENTORY
		// ------------------------
		inventoryItemIds = new List<string>();

		//if (InventoryManager.Instance != null)
		//{
		//	var items = InventoryManager.Instance.GetAll();
		//	foreach (var item in items)
		//	{
		//		if (item != null && !string.IsNullOrEmpty(item.id))
		//			inventoryItemIds.Add(item.id);
		//	}
		//}

		// ------------------------
		// GAME FLAGS
		// ------------------------
		gameFlags = new List<string>();

		if (GameFlags.Instance != null)
		{
			gameFlags = GameFlags.Instance.GetAllFlags();
		}

	}
}