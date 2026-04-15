using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;

// This script manages the switching between different Cinemachine cameras in the scene.
// It keeps track of all registered cameras and allows switching between them by setting their priority.
public static class CameraSwitcher
{
	static List<CinemachineCamera> cameras = new List<CinemachineCamera>();

	public static CinemachineCamera ActiveCamera = null;

	public static bool IsActiveCamera(CinemachineCamera camera)
	{
			return camera = ActiveCamera;
	}

	public static void SwitchCamera(CinemachineCamera camera)
	{
		camera.Priority = 10;
		ActiveCamera = camera;

		foreach (CinemachineCamera c in cameras)
		{
			if (c != camera && c.Priority != 0)
			{
				c.Priority = 0;
			}
		}
	}

	public static void Register(CinemachineCamera camera)
	{
		cameras.Add(camera);
		//Debug.Log("Registered camera: " + camera);
	}

	public static void Unregister(CinemachineCamera camera)
	{
		cameras.Remove(camera);
		//Debug.Log("Unregistered camera: " + camera);
	}
}
