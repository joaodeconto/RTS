using UnityEngine;
using System.Collections;
using Visiorama;

public class GhostFactory : MonoBehaviour
{
	protected Worker worker;
	protected TouchController touchController;
	protected FogOfWar fogOfWar;
	protected GameplayManager gameplayManager;
	
	protected FactoryBase thisFactory;
	protected GameObject overdrawModel;
	protected int numberOfCollisions = 0;
	
	public void Init (Worker worker)
	{
		this.worker = worker;
		worker.Deselect ();
		
		thisFactory = GetComponent<FactoryBase>();
		thisFactory.photonView.RPC ("InstanceOverdraw", PhotonTargets.AllBuffered, worker.Team);
		
		ComponentGetter.Get<SelectionController> ().enabled = false;
		fogOfWar = ComponentGetter.Get<FogOfWar> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		touchController = ComponentGetter.Get<TouchController> ();
		
		touchController.DisableDragOn = true;
		
		collider.isTrigger = true;
		
		gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		
		if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = false;
		
		SetOverdraw (worker.Team);
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
				if (numberOfCollisions == 0 && fogOfWar.IsVisitedPosition (transform))
				{
					Apply ();
				}
				else
				{
					Debug.Log (numberOfCollisions);
				}
			}
		}
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			numberOfCollisions++;
		}
		
		if (numberOfCollisions == 1)
		{
			SetColorOverdraw (new Color (0.25f, 0f, 0f));
		}
	}
	
	void OnTriggerExit (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			numberOfCollisions--;
		}
		
		if (numberOfCollisions == 0)
		{
			SetColorOverdraw (new Color (0f, 0.25f, 0f));
		}
	}
	
	void Apply ()
	{
		collider.isTrigger = false;
		thisFactory.enabled = true;
		
		if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = true;
		
		ComponentGetter.Get<SelectionController> ().enabled = true;
		
		thisFactory.photonView.RPC ("Instance", PhotonTargets.AllBuffered);
		
		worker.SetMoveToFactory (GetComponent<FactoryBase> ());
		
		DestroyOverdrawModel ();
		
		Destroy(rigidbody);
		Destroy(this);
	}
	
	[RPC]
	void SetOverdraw (int teamId)
	{
		overdrawModel = Instantiate (thisFactory.model, thisFactory.model.transform.position, thisFactory.model.transform.rotation) as GameObject;
		overdrawModel.transform.parent = thisFactory.model.transform.parent;
		thisFactory.model.SetActive (false);
		
		foreach (Renderer r in overdrawModel.GetComponentsInChildren<Renderer>())
		{
			foreach (Material m in r.materials)
			{
				m.shader = Shader.Find ("Overdraw");
				m.color = new Color (0f, 0.25f, 0f);
			}
		}
	}
	
	void SetColorOverdraw (Color color)
	{
		foreach (Renderer r in overdrawModel.GetComponentsInChildren<Renderer>())
		{
			foreach (Material m in r.materials)
			{
				m.color = color;
			}
		}
	}
	
	void DestroyOverdrawModel ()
	{
		thisFactory.model.SetActive (true);
		Destroy (overdrawModel);
	}
}
