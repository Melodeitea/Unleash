using System.Collections.Generic;
using UnityEngine;

// Previously: managed priority switching between multiple CinemachineCameras.
// Now: manages zone history and delegates transitions to the single
// ZoneCameraController. The static API is preserved so call-sites stay familiar.
public static class CameraSwitcher
{
	private static ZoneCameraController _controller;
	private static readonly Stack<CameraZoneData> _history = new Stack<CameraZoneData>();
	public static CameraZoneData ActiveZone { get; private set; }

	// Called automatically by ZoneCameraController on enable/disable
	public static void SetController(ZoneCameraController controller)
	{
		_controller = controller;
	}

	// ── Main API ──────────────────────────────────────────────────────────────

	// Transition to a new zone, saving the current one in history.
	public static void SwitchToZone(CameraZoneData zone)
	{
		if (_controller == null)
		{
			Debug.LogWarning("[CameraSwitcher] No active ZoneCameraController in scene.");
			return;
		}

		if (ActiveZone != null)
			_history.Push(ActiveZone);

		ActiveZone = zone;
		_controller.TransitionToZone(zone);
	}

	// Revert to the previous zone (e.g. player walks back through a doorway).
	public static void GoToPreviousZone()
	{
		if (_history.Count == 0)
		{
			Debug.LogWarning("[CameraSwitcher] No previous zone in history.");
			return;
		}

		ActiveZone = _history.Pop();
		_controller.TransitionToZone(ActiveZone);
	}

	// Snap instantly to a zone and clear history (useful for scene resets).
	public static void SnapToZone(CameraZoneData zone)
	{
		_history.Clear();
		ActiveZone = zone;
		_controller?.SnapToZone(zone);
	}

	public static bool HasPreviousZone => _history.Count > 0;
}