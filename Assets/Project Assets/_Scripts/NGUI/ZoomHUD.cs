using UnityEngine;
using System.Collections;

public class ZoomHUD : MonoBehaviour
{
	public GameObject zoomInButton, zoomOutButton;
	
	protected float zoomSpeed = 0.2f;
	
	void Awake ()
	{
		TouchController touchController = Visiorama.ComponentGetter.Get<TouchController> ();
		
		DefaultCallbackButton dcb;
		
		if (zoomInButton != null)
		{
			Hashtable ht = new Hashtable();
			ht.Add ("repeat-interval", 0.01f);
			
			dcb = zoomInButton.AddComponent<DefaultCallbackButton> ();
			dcb.Init (ht, null, null, null, null,
			(ht_dcb) => 
			{
				float size = touchController.zoomSettings.cameras[0].orthographicSize;
				size -= zoomSpeed;
				for (int i = 0; i != touchController.zoomSettings.cameras.Length; i++)
				{
					touchController.zoomSettings.cameras[i].orthographicSize = Mathf.Clamp (size, 
																							touchController.zoomSettings.zoom.min,
																							touchController.zoomSettings.zoom.max);
				}
			});
		}
		
		if (zoomOutButton != null)
		{
			Hashtable ht = new Hashtable();
			ht.Add ("repeat-interval", 0.01f);
			
			dcb = zoomInButton.AddComponent<DefaultCallbackButton> ();
			dcb.Init (ht, null, null, null, null,
			(ht_dcb) => 
			{
				float size = touchController.zoomSettings.cameras[0].orthographicSize;
				size += zoomSpeed;
				for (int i = 0; i != touchController.zoomSettings.cameras.Length; i++)
				{
					touchController.zoomSettings.cameras[i].orthographicSize = Mathf.Clamp (size, 
																							touchController.zoomSettings.zoom.min,
																							touchController.zoomSettings.zoom.max);
				}
			});
		}
	}
}