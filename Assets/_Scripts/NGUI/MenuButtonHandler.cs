using UnityEngine;
using System.Collections;

public class MenuButtonHandler : MonoBehaviour
{
	public GameObject mainMenuObject;


	void OnClick ()
	{
		mainMenuObject.SetActive (true);
	}
}