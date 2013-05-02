using UnityEngine;
using System.Collections;

public class MenuButtonHandler : MonoBehaviour
{
	public GameObject audioOptionsObject;
	
	void OnClick ()
	{
		audioOptionsObject.SetActive (true);
	}
}