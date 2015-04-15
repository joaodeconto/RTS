using UnityEngine;
using UnityEngine.Cloud.Analytics;
using System.Collections;
using System.Collections.Generic;

public class ExitGame : MonoBehaviour 
{
	private bool wasInitialized = false;

	public void OnEnable ()
	{
		Open ();
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
				Time.timeScale = 1f;
				GameplayManager gm = Visiorama.ComponentGetter.Get<GameplayManager>();
				if(gm.scoreCounting) gm.Surrender();
				Close ();
				InGameMenu gameMenu = transform.parent.GetComponentInChildren<InGameMenu>();
				gameMenu.Close();
									
			});						

		}
		
		Transform no = transform.FindChild ("No");
		
		if (no != null)
		{
			dcb = no.gameObject.AddComponent<DefaultCallbackButton> ();
			
			dcb.Init (null, (ht) =>
			{
				Close();
			}
			);
		}
		
		gameObject.SetActive (false);
	}

	public void Close ()
	{		
		gameObject.SetActive (false);
	}
}