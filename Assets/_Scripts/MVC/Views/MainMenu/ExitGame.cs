using UnityEngine;
using System.Collections;

public class ExitGame : MonoBehaviour 
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
		
		Transform yes = transform.FindChild ("Yes");
		
		if (yes != null)
		{
			dcb = yes.gameObject.AddComponent<DefaultCallbackButton> ();
			
			dcb.Init (null, (ht) =>
			{
				if (PhotonNetwork.room != null)
					PhotonNetwork.LeaveRoom ();

				Time.timeScale = 1f;
				Application.LoadLevel (0);
			}
			);
		}
		
		Transform no = transform.FindChild ("No");
		
		if (no != null)
		{
			dcb = no.gameObject.AddComponent<DefaultCallbackButton> ();
			
			dcb.Init (null, (ht) =>
			{
				gameObject.SetActive (false);
			}
			);
		}
		
		gameObject.SetActive (false);
	}

	public void Close ()
	{
		
	}
}