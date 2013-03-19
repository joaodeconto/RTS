using UnityEngine;
using System.Collections;
using Visiorama;

public class GhostFactory : MonoBehaviour
{
	protected Worker worker;
	protected TouchController touchController;
	protected FogOfWar fogOfWar;
	
	public void Init (Worker worker)
	{
		this.worker = worker;
		worker.Deselect ();
		GetComponent<FactoryBase>().Instance (worker.Team);
		ComponentGetter.Get<SelectionController> ().enabled = false;
		ComponentGetter.Get<FactoryController> ().RemoveFactory (GetComponent<FactoryBase> ());
		fogOfWar = ComponentGetter.Get<FogOfWar> ();
		touchController = ComponentGetter.Get<TouchController> ();
		
		touchController.DisableDragOn = true;
		
		collider.isTrigger = true;
		
		gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		
		if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = false;
	}
	
	void Update ()
	{
		Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		// Patch transform com hit.point
		transform.position = Vector3.zero;
		
		if (Physics.Raycast (ray, out hit))
		{
			transform.position = hit.point;	
		}
				
		if (touchController.touchType == TouchController.TouchType.Ended)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id0)
			{
				if (i == 0 && fogOfWar.IsVisitedPosition (transform))
				{
					Apply ();
				}
				else
				{
					Debug.Log (i);
				}
			}
		}
	}
	
	int i = 0;
	void OnTriggerEnter (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			i++;
		}
	}
	
	void OnTriggerExit (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			i--;
		}
	}
	
	void Apply ()
	{
		collider.isTrigger = false;
		GetComponent<FactoryBase>().enabled = true;
		if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = true;
		
		ComponentGetter.Get<FactoryController> ().AddFactory (GetComponent<FactoryBase> ());
		ComponentGetter.Get<SelectionController> ().enabled = true;
		
		worker.SetMoveToFactory (GetComponent<FactoryBase> ());
		
		Destroy(rigidbody);
		Destroy(this);
	}
}
