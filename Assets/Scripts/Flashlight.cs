using UnityEngine;

[DisallowMultipleComponent]
public class Flashlight : MonoBehaviour
{
	[Header("Input")]
	public KeyCode toggleKey = KeyCode.A;

	[Header("Light")]
	public Light spotLight;

	[Header("Rotation")]
	public float sensitivity = 2f;
	public float minPitch = -60f;
	public float maxPitch = 60f;

	bool _isOn;
	float _pitch;
	float _yaw;

	public bool IsOn => _isOn;

	void Reset()
	{
		spotLight = GetComponentInChildren<Light>();
	}

	void Start()
	{
		if (spotLight == null)
			spotLight = GetComponentInChildren<Light>();

		// initialize rotation from transform
		var e = transform.localEulerAngles;
		_pitch = NormalizeAngle(e.x);
		_yaw = NormalizeAngle(e.y);

		ApplyLightState();
	}

	void Update()
	{
		// toggle
		if (Input.GetKeyDown(toggleKey))
		{
			Toggle();
		}

		// rotate with mouse only when cursor is locked and flashlight is on
		if (_isOn && Cursor.lockState == CursorLockMode.Locked)
		{
			float mx = Input.GetAxis("Mouse X");
			float my = Input.GetAxis("Mouse Y");

			_yaw += mx * sensitivity;
			_pitch -= my * sensitivity; // invert Y for natural look
			_pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

			transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
		}
	}

	public void Toggle()
	{
		SetState(!_isOn);
	}

	public void SetState(bool on)
	{
		_isOn = on;
		ApplyLightState();
	}

	void ApplyLightState()
	{
		if (spotLight != null)
			spotLight.enabled = _isOn;
	}

	public void SetRotationEuler(Vector3 euler)
	{
		_pitch = NormalizeAngle(euler.x);
		_yaw = NormalizeAngle(euler.y);
		transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
	}

	public Vector3 GetRotationEuler()
	{
		return new Vector3(_pitch, _yaw, 0f);
	}

	static float NormalizeAngle(float a)
	{
		a %= 360f;
		if (a > 180f) a -= 360f;
		return a;
	}
}
