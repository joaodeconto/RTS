using UnityEngine;
using System.Collections;

public class ZoomHUD : MonoBehaviour
{
	public GameObject zoomInButton, zoomOutButton;
	
	protected float zoomSpeed = 0.2f;
	
	void Awake ()
	{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR		
		TouchController touchController = Visiorama.ComponentGetter.Get<TouchController> ();
		
		DefaultCallbackButton dcb;
		
		if (zoomInButton != null)
		{
			dcb = zoomInButton.AddComponent<DefaultCallbackButton> ();
			dcb.Init (null, null, null, null, null,
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
			dcb = zoomOutButton.AddComponent<DefaultCallbackButton> ();
			dcb.Init (null, null, null, null, null,
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
#else
		zoomInButton.SetActive (false);
		zoomOutButton.SetActive (false);
#endif
	}
}