using UnityEngine;
using Unity.Cinemachine;


//this registers the cameras with the CameraSwitcher script, so that they can be switched between
public class CameraRegister : MonoBehaviour
{
	private void OnEnable()
	{
		      CameraSwitcher.Register(GetComponent<CinemachineCamera>());
	}

	private void OnDisable()
	{
			  CameraSwitcher.Unregister(GetComponent<CinemachineCamera>());
	}
}

