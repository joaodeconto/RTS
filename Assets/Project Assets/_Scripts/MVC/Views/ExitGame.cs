using UnityEngine;
using System.Collections;

public class ExitGame : MonoBehaviour {
	
	void Awake ()
	{
		DefaultCallbackButton dcb;
		
		Transform yes = transform.FindChild ("Yes");
		
		if (yes != null)
		{
			dcb = yes.gameObject.AddComponent<DefaultCallbackButton> ();
			
			dcb.Init (null, (ht) =>
			{
				if (PhotonNetwork.room != null)
					PhotonNetwork.LeaveRoom ();
				
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
}