using UnityEngine;
using System.Collections;
using Visiorama;

public class CreationPoint : MonoBehaviour {
	
	public bool active {get; protected set;}
	
	protected TouchController touchController;
	protected LineRenderer lineRenderer;
	
	// Use this for initialization
	void Awake ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.enabled = active = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (active)
		{
			lineRenderer.SetPosition (0, transform.position);
			lineRenderer.SetPosition (1, transform.parent.position);
			if (touchController.touchType != TouchController.TouchType.First)
			{
				Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				
				int layerMask = 1 << LayerMask.NameToLayer("Terrain");
				
				// Patch transform com hit.point
				transform.position = Vector3.zero;
				
				if (Physics.Raycast (ray, out hit, layerMask))
				{
					transform.position = hit.point;
				}
			}
			else
			{
				if (touchController.idTouch == TouchController.IdTouch.Id1)
				{
					Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
					RaycastHit hit;
					
					int layerMask = 1 << LayerMask.NameToLayer("Terrain");
					
					// Patch transform com hit.point
					transform.position = Vector3.zero;
					
					if (Physics.Raycast (ray, out hit, layerMask))
					{
						transform.position = hit.point;
					}
					
					lineRenderer.enabled = active = false;
				}
			}
		}
		else
		{
			if (touchController.touchType == TouchController.TouchType.First)
			{
				if (touchController.idTouch == TouchController.IdTouch.Id1)
				{
					if (touchController.GetFirstRaycastHit.transform == transform)
					{
						touchController.DisableDragOn = true;
						lineRenderer.enabled = active = true;
					}
				}
			}
		}
	}
}
