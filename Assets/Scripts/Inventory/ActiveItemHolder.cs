// No MonoBehaviour needed — just a static slot
public static class ActiveItemHolder
{
	public static ItemData Current;

	public static void Clear() => Current = null;
}