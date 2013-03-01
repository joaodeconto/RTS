using UnityEngine;
using System.Collections;
using Visiorama;

public class CreationPoint : MonoBehaviour {
	
	protected TouchController touchController;
	
	// Use this for initialization
	void Awake ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
	}
	
	bool active = false;
	// Update is called once per frame
	void Update ()
	{
		if (active)
		{
			if (touchController.touchType == TouchController.TouchType.Press)
			{
				transform.position = touchController.mainCamera.ScreenToWorldPoint (touchController.CurrentPosition);
			}
			else
			{
				active = false;
			}
		}
		
		if (touchController.touchType == TouchController.TouchType.First)
		{
			if (touchController.GetFirstRaycastHit.transform == transform)
			{
				active = true;
			}
		}
	}
}
