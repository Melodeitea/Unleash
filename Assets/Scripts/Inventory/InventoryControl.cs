using System.Collections;
using UnityEngine;

public class InventoryControl : MonoBehaviour
{

	public GameObject inventoryScreen;
	public GameObject inventoryFade;
	public AudioSource inventoryOpen;
	public bool isOpen = false;
	public AudioSource inventoryClose;
	public bool canClose = false;


	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab) && isOpen == false && canClose == false)
		{
			// Debug.Log("Tab key was pressed.");

			isOpen = true;
			inventoryOpen.Play();
			inventoryFade.SetActive(true);
			StartCoroutine(InvControl());
		}

		if (Input.GetKeyDown(KeyCode.Tab) && isOpen == true && canClose == true)
		{
			isOpen = false;
			inventoryClose.Play();
			inventoryFade.SetActive(true);
			StartCoroutine(InvControl());
		}
	}

	IEnumerator InvControl()
	{
		yield return new WaitForSeconds(0.25f);
		if (isOpen == true)
		{
			inventoryScreen.SetActive(true);

		}
		else
		{
			inventoryScreen.SetActive(false);
		}
		yield return new WaitForSeconds(0.25f);
		inventoryFade.SetActive(false);
		
		if (isOpen == true)
		{
			canClose = true;
		}
		else
		{
			canClose = false;
		}
	}

}
