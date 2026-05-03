using System.Collections.Generic;
using UnityEngine;

public class RedLayerManager : MonoBehaviour
{
	public static RedLayerManager Instance { get; private set; }

	private List<RedObjectRegistration> objects = new();

	public bool IsActive { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public void Register(RedObjectRegistration obj)
	{
		if (!objects.Contains(obj))
		{
			objects.Add(obj);
			obj.SetState(IsActive);
		}
	}

	public void Deregister(RedObjectRegistration obj)
	{
		objects.Remove(obj);
	}

	public void ToggleLayer()
	{
		IsActive = !IsActive;

		foreach (var obj in objects)
		{
			obj.SetState(IsActive);
		}
	}

	public void ActivateAll()
	{
		IsActive = true;

		foreach (var obj in objects)
		{
			obj.SetState(true);
		}
	}

	public void SetLayerState(bool active)
	{
		IsActive = active;

		foreach (var obj in objects)
		{
			obj.SetState(active);
		}
	}
}