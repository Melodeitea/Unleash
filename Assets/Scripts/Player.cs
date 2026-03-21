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

		// apply puzzles
		if (PuzzleManager.Instance != null && data.solvedPuzzles != null)
		{
			PuzzleManager.Instance.SetSolvedList(data.solvedPuzzles);
			// notify existing Puzzle components so they can update visuals
			var allPuzzles = FindObjectsOfType<Puzzle>();
			foreach (var p in allPuzzles)
			{
				if (!string.IsNullOrEmpty(p.puzzleId) && PuzzleManager.Instance.IsSolved(p.puzzleId))
				{
					// ensure internal flag and apply visuals
					p.isSolved = true;
					// call protected method by using Solve replacement? call its OnApplySolvedState via Solve would also mark manager again.
					// Instead, invoke Solve's effects by calling virtual handler via reflection or a public method - here we call SolveSafe:
					p.SendMessage("OnApplySolvedState", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}
