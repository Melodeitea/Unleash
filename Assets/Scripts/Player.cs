using UnityEngine;

public class Player : MonoBehaviour
{
	public int level;

	public void SavePlayer()
	{
		SaveSystem.SavePlayer(this);
	}

	public void LoadPlayer()
	{
		PlayerData data = SaveSystem.LoadPlayer();
		if (data == null)
		{
			Debug.LogWarning("No save to load.");
			return;
		}

		level = data.level;

		if (data.position != null && data.position.Length >= 3)
		{
			Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
			transform.position = position;
		}

		// apply flashlight state if present
		var fl = FindObjectOfType<Flashlight>();
		if (fl != null)
		{
			fl.SetState(data.flashlightOn);
			if (data.flashlightEuler != null && data.flashlightEuler.Length >= 2)
			{
				fl.SetRotationEuler(new Vector3(data.flashlightEuler[0], data.flashlightEuler[1], 0f));
			}
		}

		// apply puzzles via PuzzleSystem (replaces previous Puzzle / PuzzleManager direct calls)
		var puzzleSystem = FindObjectOfType<PuzzleSystem>();
		if (puzzleSystem != null && data.solvedPuzzles != null)
		{
			puzzleSystem.ApplySolvedIds(data.solvedPuzzles);
		}
	}
}
