using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
	private const string SaveFileName = "player.save";

	static string GetSavePath()
	{
		return Path.Combine(Application.persistentDataPath, SaveFileName);
	}

	public static void SavePlayer(Player player)
	{
		if (player == null)
		{
			Debug.LogError("[SaveSystem] SavePlayer called with null player.");
			return;
		}

		try
		{
			PlayerData data = new PlayerData(player);

			string json = JsonUtility.ToJson(data, true);

			string path = GetSavePath();
			string tmp = path + ".tmp";

			File.WriteAllText(tmp, json);

			if (File.Exists(path))
				File.Delete(path);

			File.Move(tmp, path);

			Debug.Log($"[SaveSystem] Save successful: {path}");
		}
		catch (Exception ex)
		{
			Debug.LogError($"[SaveSystem] Save failed: {ex}");
		}
	}

	public static PlayerData LoadPlayer()
	{
		string path = GetSavePath();

		if (!File.Exists(path))
		{
			Debug.LogWarning("[SaveSystem] No save file found.");
			return null;
		}

		try
		{
			string json = File.ReadAllText(path);
			PlayerData data = JsonUtility.FromJson<PlayerData>(json);

			if (data == null)
			{
				Debug.LogError("[SaveSystem] Failed to parse save file.");
				return null;
			}

			return data;
		}
		catch (Exception ex)
		{
			Debug.LogError($"[SaveSystem] Load failed: {ex}");
		}

		return null;
	}

	public static bool HasSave()
	{
		string path = GetSavePath();
		return File.Exists(path);
	}
}