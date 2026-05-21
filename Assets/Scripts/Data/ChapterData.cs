using UnityEngine;

[CreateAssetMenu(menuName = "Chapters/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public int chapterNumber;
    public string chapterTime;         // e.g. "2:00 AM"
    public string displayName;         // e.g. "The Body"
    public string[] requiredFlags;
    public string nextSceneName;
}