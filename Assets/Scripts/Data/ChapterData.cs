using UnityEngine;

[CreateAssetMenu(menuName = "Chapters/Chapter Data")]
public class ChapterData : ScriptableObject
{
	public int chapterNumber;
	public string displayName;         // e.g. "Chapter Two"
	public string[] requiredFlags;     // all must be set to complete chapter
	public string nextSceneName;       // scene to load after transition, "Credits" for last
}