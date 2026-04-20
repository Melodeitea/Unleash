using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
	public int level;
	public float[] position;

	// new fields persisted
	public bool flashlightOn;
	public float[] flashlightEuler;
	public List<string> solvedPuzzles;

	public PlayerData(Player player)
	{
		level = player.level;
		position = new float[3];
		position[0] = player.transform.position.x;
		position[1] = player.transform.position.y;
		position[2] = player.transform.position.z;

		// flashlight
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

		// puzzles: read from PuzzleSystem (no PuzzleManager fallback)
		solvedPuzzles = new List<string>();
		var ps = UnityEngine.Object.FindObjectOfType<PuzzleSystem>();
		if (ps != null)
		{
			var list = ps.GetSolvedPuzzleIds();
			if (list != null) solvedPuzzles = list;
		}
	}
}

