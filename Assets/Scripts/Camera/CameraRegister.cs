using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

// Previously: registered a CinemachineCamera with CameraSwitcher.
// Now: owns the single CinemachineCamera and drives it between zone positions.
// Attach this to your one CinemachineCamera GameObject.
// On that camera remove (or set to "Do Nothing") any procedural Position /
// Rotation Control components so this script can move it freely.
public class ZoneCameraController : MonoBehaviour
{
	public static ZoneCameraController Instance { get; private set; }

	private CinemachineCamera _cam;
	private Coroutine _activeTransition;

	private void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(this); return; }
		Instance = this;

		_cam = GetComponent<CinemachineCamera>();
		if (_cam == null)
			Debug.LogError("[ZoneCameraController] No CinemachineCamera found on this GameObject.", this);
	}

	// Hook into CameraSwitcher when the GameObject is active
	private void OnEnable() => CameraSwitcher.SetController(this);
	private void OnDisable() => CameraSwitcher.SetController(null);

	// ── Called by CameraSwitcher ──────────────────────────────────────────────

	public void TransitionToZone(CameraZoneData zone)
	{
		if (_activeTransition != null) StopCoroutine(_activeTransition);
		_activeTransition = StartCoroutine(RunTransition(zone));
	}

	public void SnapToZone(CameraZoneData zone)
	{
		if (_activeTransition != null) StopCoroutine(_activeTransition);
		_activeTransition = null;
		ApplyImmediate(zone);
	}

	// ── Transition coroutine ──────────────────────────────────────────────────

	private IEnumerator RunTransition(CameraZoneData target)
	{
		Transform t = _cam.transform;
		Vector3 startPos = t.position;
		Quaternion startRot = t.rotation;
		float startFOV = GetFOV();
		float elapsed = 0f;
		float duration = Mathf.Max(target.transitionDuration, 0.001f);

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float eased = target.transitionCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

			t.position = Vector3.Lerp(startPos, target.position, eased);
			t.rotation = Quaternion.Slerp(startRot, target.Rotation, eased);
			SetFOV(Mathf.Lerp(startFOV, target.fieldOfView, eased));
			yield return null;
		}

		ApplyImmediate(target);
		_activeTransition = null;
	}

	private void ApplyImmediate(CameraZoneData zone)
	{
		_cam.transform.SetPositionAndRotation(zone.position, zone.Rotation);
		SetFOV(zone.fieldOfView);
	}

	// ── Lens helpers ──────────────────────────────────────────────────────────

	private float GetFOV() => _cam != null ? _cam.Lens.FieldOfView : 60f;

	private void SetFOV(float fov)
	{
		if (_cam == null) return;
		LensSettings lens = _cam.Lens;
		lens.FieldOfView = fov;
		_cam.Lens = lens;
	}
}