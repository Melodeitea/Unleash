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

			// atomic write: write temp then move/replace
			File.WriteAllText(tmp, json);
			if (File.Exists(path)) File.Delete(path);
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
			Debug.LogError("[SaveSystem] Save file not found in " + path);
			return null;
		}

		try
		{
			// Peek first non-whitespace byte to determine JSON vs binary (backwards compatibility)
			using (var fs = File.OpenRead(path))
			{
				int b;
				do
				{
					b = fs.ReadByte();
					if (b == -1) break;
				} while (char.IsWhiteSpace((char)b));

				if (b == -1)
				{
					Debug.LogError("[SaveSystem] Save file is empty or corrupted.");
					return null;
				}

				fs.Seek(0, SeekOrigin.Begin);

				char first = (char)b;
				if (first == '{' || first == '[')
				{
					// JSON format
					string json = File.ReadAllText(path);
					PlayerData data = JsonUtility.FromJson<PlayerData>(json);
					return data;
				}
				else
				{
					// Fallback to BinaryFormatter for old saves, then migrate to JSON
					try
					{
						var formatter = new BinaryFormatter();
						var obj = formatter.Deserialize(fs) as PlayerData;
						if (obj != null)
						{
							// migrate to JSON
							try
							{
								string json = JsonUtility.ToJson(obj, true);
								File.WriteAllText(path, json);
								Debug.Log("[SaveSystem] Migrated legacy binary save to JSON format.");
							}
							catch (Exception)
							{
								// ignore migration errors
							}

							return obj;
						}
					}
					catch (Exception bfEx)
					{
						Debug.LogWarning($"[SaveSystem] Binary fallback failed: {bfEx}. Trying JSON read as last resort.");
						// try JSON read anyway
						string json = File.ReadAllText(path);
						PlayerData data = JsonUtility.FromJson<PlayerData>(json);
						return data;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"[SaveSystem] Load failed: {ex}");
		}

		return null;
	}

	// Returns true when a save file exists and is accessible.
	public static bool HasSave()
	{
		string path = GetSavePath();
		return File.Exists(path);
	}
}
