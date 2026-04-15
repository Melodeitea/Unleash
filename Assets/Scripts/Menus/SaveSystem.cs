using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public static class SaveSystem

{
	public static void SavePlayer(Player player)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        //alows to save on any OS
        string path = Application.persistentDataPath + "/player.save";

        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        Debug.Log("project saved");
        Debug.Log("progress was saved at" + path);
        stream.Close();
    }
    
    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.save";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();
            

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);

            return null;
        }
	}

	// Returns true when a save file exists and is accessible.
	// for main menu load button to only show when a save exists
	public static bool HasSave()
	{
		string path = Application.persistentDataPath + "/player.save";
		return File.Exists(path);
	}
}
