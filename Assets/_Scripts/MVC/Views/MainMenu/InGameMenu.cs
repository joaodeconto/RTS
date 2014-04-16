using UnityEngine;
using System.Collections;
using Visiorama;

public class InGameMenu : MonoBehaviour
{
	private bool wasInitialized = false;

	public GameObject controlsOptionPanel;
	public GameObject audioOptionPanel;
	public GameObject surrenderPanel;


	
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
		
		Transform controls = this.transform.FindChild ("Controls");

		if (controls != null)
		{
			dcb = controls.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {

				controlsOptionPanel.SetActive (true);

				gameObject.SetActive (false);
			});
		}

		Transform audio = this.transform.FindChild ("Audio");
		
		if (audio != null)
		{
			dcb = audio.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				audioOptionPanel.SetActive (true);
				
				gameObject.SetActive (false);
			});
		}

		Transform quit = this.transform.FindChild ("Surrender");
		
		if (quit != null)
		{
			dcb = quit.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				surrenderPanel.SetActive (true);
				
				gameObject.SetActive (false);
			});
		}
		

		
		Transform close = this.transform.FindChild ("Resume");
		
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
