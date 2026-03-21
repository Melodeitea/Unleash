using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

// This script defines a trigger volume that switches the active Cinemachine camera when the player enters it.
// It uses a BoxCollider set as a trigger to detect when the player enters the volume,
// and then it calls the CameraSwitcher to switch to the specified camera.
public class CameraTriggerVolume : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private Vector3 boxSize;

    BoxCollider box;
    Rigidbody rb;


	private void Awake()
	{
		box = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        box.isTrigger = true;
        box.size = boxSize;

        rb.isKinematic = true;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
	}

	private void OnTriggerEnter(Collider other)
	{
		      if (other.gameObject.CompareTag("Player"))
        {
            if (CameraSwitcher.ActiveCamera != cam) CameraSwitcher.SwitchCamera(cam);
		}
	}
}
