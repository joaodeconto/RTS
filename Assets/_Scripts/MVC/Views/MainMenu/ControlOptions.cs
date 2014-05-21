using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class ControlOptions : MonoBehaviour
{
	private bool wasInitialized = false;
	

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



		DefaultCallbackButton dcb;

		Transform close = this.transform.FindChild ("Menu").FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			(ht_dcb) => 
			{
				gameObject.SetActive (false);
			});
		}
	}

	public void Close ()
	{

	}
}
