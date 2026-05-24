using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{

	public int currentChapterIndex;

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

		// INVENTORY
		inventoryItemIds = new List<string>();
		if (InventoryManager.Instance != null)
		{
			foreach (var item in InventoryManager.Instance.items)
				if (!string.IsNullOrEmpty(item.itemID)) inventoryItemIds.Add(item.itemID);

			foreach (var file in InventoryManager.Instance.notes)
				if (!string.IsNullOrEmpty(file.itemID)) inventoryItemIds.Add(file.itemID);

			foreach (var clue in InventoryManager.Instance.clues)
				if (!string.IsNullOrEmpty(clue.itemID)) inventoryItemIds.Add(clue.itemID);
		}

		// ------------------------
		// GAME FLAGS
		// ------------------------
		gameFlags = new List<string>();

		if (GameFlags.Instance != null)
		{
			gameFlags = GameFlags.Instance.GetAllFlags();
		}

		currentChapterIndex = ChapterManager.Instance != null
		? ChapterManager.Instance.currentChapterIndex : 0;

	}
}