using UnityEngine;
using System.Collections;

public class AboutMenu : MonoBehaviour
{
	private bool wasInitialized = false;

	Transform buttons;

	public void OnEnable ()
	{
		Open ();
	}

	public void OnDisable ()
	{
		Close ();
	}

	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;


		buttons = this.transform.FindChild ("Menu").FindChild ("Buttons");

	}

	public void Close ()
	{

	}
}
