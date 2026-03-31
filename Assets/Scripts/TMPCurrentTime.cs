using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class TMPCurrentTime : MonoBehaviour
{
	[SerializeField] TMP_Text currentTime;


	// Update is called once per frame
	void Update()
	{
		currentTime.text = DateTime.Now.ToString("HH:mm");
	}
}
