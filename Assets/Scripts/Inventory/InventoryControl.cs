using System.Collections;
using UnityEngine;

public class InventoryControl : MonoBehaviour
{

	public GameObject inventoryScreen;
	public GameObject inventoryFade;
	public AudioSource inventoryOpen;

	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Debug.Log("Tab key was pressed.");
			
			inventoryOpen.Play();
			inventoryFade.SetActive(true);
			StartCoroutine(InvOpen());
		}
	}

	IEnumerator InvOpen()
	{
		yield return new WaitForSeconds(0.25f);
		inventoryScreen.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		inventoryFade.SetActive(false);
	}

}
