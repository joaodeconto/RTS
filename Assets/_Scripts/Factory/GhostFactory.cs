using UnityEngine;
using System.Collections;
using Visiorama;

public class GhostFactory : MonoBehaviour
{
	protected Worker worker;
	protected TouchController touchController;
	protected GameplayManager gameplayManager;

	protected string correctName;
	protected Worker.FactoryConstruction factoryConstruction;
	protected FactoryBase thisFactory;
	protected GameObject overdrawModel;
	protected int numberOfCollisions = 0;
	protected float realRadius;
	public bool collideOnNavMeshLayer;
	
	private bool isCapsuleCollider;

	private int terrainLayer = (1 << LayerMask.NameToLayer ("Terrain"));
	private int navMeshLayer = (1 << NavMesh.GetNavMeshLayerFromName ("Default"));

	public void Init (Worker worker, Worker.FactoryConstruction factoryConstruction)
	{
		GameObject oldGhost = GameObject.Find ("GhostFactory");
		if (oldGhost != null) Destroy (oldGhost);
		
		this.worker 			 = worker;
		this.factoryConstruction = factoryConstruction;

		thisFactory = GetComponent<FactoryBase>();
		thisFactory.photonView.RPC ("InstanceOverdraw", PhotonTargets.All, worker.team, worker.ally);
		
		correctName = thisFactory.name;
		thisFactory.name = "GhostFactory";

		ComponentGetter.Get<InteractionController> ().enabled = false;
		ComponentGetter.Get<SelectionController> ().enabled = false;
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		touchController = ComponentGetter.Get<TouchController> ();

		touchController.DisableDragOn = true;
		
		isCapsuleCollider = true;

		thisFactory.gameObject.layer = LayerMask.NameToLayer ("Gizmos");
		
		GameObject helperColliderGameObject;
		
		if (gameObject.GetComponent<CapsuleCollider> () == null)
		{
			isCapsuleCollider = false;
			
//			CapsuleCollider capCollider = gameObject.GetComponent<CapsuleCollider> ();
//			capCollider.radius = thisFactory.helperCollider.radius;
//			capCollider.center = thisFactory.helperCollider.center;
//			capCollider.height = thisFactory.helperCollider.height;
			
			thisFactory.helperCollider.isTrigger = true;
			realRadius = thisFactory.helperCollider.radius;
		//	thisFactory.helperCollider.radius += 3f;
			
			helperColliderGameObject = thisFactory.helperCollider.gameObject;
			
			numberOfCollisions--;
		}
		else
		{
			GetComponent<CapsuleCollider> ().isTrigger = true;
			realRadius = GetComponent<CapsuleCollider> ().radius;
	//		GetComponent<CapsuleCollider> ().radius += 3f;
			
			helperColliderGameObject = gameObject;
		}
		
		helperColliderGameObject.AddComponent<Rigidbody> ();
		helperColliderGameObject.rigidbody.isKinematic = true;
		
		HelperColliderDetect hcd = helperColliderGameObject.AddComponent<HelperColliderDetect> ();
		hcd.Init
		(
			(other) =>
			{
				OnCollider (other);
			},
			(other) => 
			{
				OffCollider (other);
			}
		);
		
		SetOverdraw (worker.team);
	}

	void Update ()
	{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if (touchController.idTouch == TouchController.IdTouch.Id1) return;
#endif
		
		Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		NavMeshHit navHit;


		// Patch transform com hit.point
	//	transform.position = Vector3.zero;


		if (Physics.Raycast (ray, out hit, Mathf.Infinity, terrainLayer))
		{
			transform.position = hit.point;
		}
		collideOnNavMeshLayer = NavMesh.SamplePosition (hit.point, out navHit, 0.1f, 1);
//		Debug.DrawRay (ray.origin,ray.direction * 10000f);
//		Debug.Log (collideOnNavMeshLayer);


		if (touchController.touchType == TouchController.TouchType.Ended)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id0)
			{
		//		collideOnNavMeshLayer = NavMesh.SamplePosition (hit.point, out navHit, 1f, 1);
				
							

//				Debug.Break ();

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (numberOfCollisions == 0)
#else
				if (numberOfCollisions == 0 && collideOnNavMeshLayer == true)
#endif
				{
					Apply ();
				}
				else
				{

				}
			}
			else
			{
				ComponentGetter.Get<SelectionController> ().enabled = true;
				ComponentGetter.Get<InteractionController> ().enabled = true;
				PhotonNetwork.Destroy (gameObject);
			}
		}
		if (numberOfCollisions != 0 || collideOnNavMeshLayer == false)
		{
			SetColorOverdraw (new Color (0.75f, 0.25f, 0.25f));
		}

		if (numberOfCollisions == 0 && collideOnNavMeshLayer == true)
		{
			SetColorOverdraw (new Color (0.25f, 0.75f, 0.25f));
		}
	}
	
	void OnCollider (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			numberOfCollisions++;
		}


	}

	void OffCollider (Collider other)
	{
		if (!other.name.Equals ("Terrain"))
		{
			numberOfCollisions--;
		}



	}
	
	void Apply ()
	{
		ComponentGetter.Get<SelectionController> ().enabled = true;
		ComponentGetter.Get<InteractionController> ().enabled = true;

		bool canBuy = gameplayManager.resources.CanBuy (factoryConstruction.costOfResources);

		if (canBuy)
		{
			gameplayManager.resources.UseResources (factoryConstruction.costOfResources);//. factoryConstruction.costOfResources);
		
			GameObject helperColliderGameObject;
			
			if (isCapsuleCollider)
			{
				GetComponent<CapsuleCollider> ().radius = realRadius;
				
				helperColliderGameObject = gameObject;
			}
			else
			{
				thisFactory.helperCollider.radius = realRadius;
				
				helperColliderGameObject = thisFactory.helperCollider.gameObject;
			}
			
			Destroy (helperColliderGameObject.rigidbody);
			Destroy (helperColliderGameObject.GetComponent<HelperColliderDetect> ());

			collider.isTrigger = false;
			thisFactory.enabled = true;

			thisFactory.name = correctName;

			thisFactory.GetComponent<PaintAgent>().Paint ();
			
			if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = true;

			thisFactory.photonView.RPC ("Instance", PhotonTargets.All);
			gameObject.SendMessage ("OnInstance", SendMessageOptions.DontRequireReceiver);
			
			thisFactory.costOfResources = factoryConstruction.costOfResources;

			worker.SetMoveToFactory (thisFactory);
			StatsController troopController = ComponentGetter.Get<StatsController> ();
			foreach (Unit unit in troopController.selectedStats)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker otherWorker = unit as Worker;
					otherWorker.SetMoveToFactory (thisFactory);
				}
			}

			DestroyOverdrawModel ();

			Destroy(this);
		}
		else
		{
			PhotonNetwork.Destroy (gameObject);
		}
	}

	void SetOverdraw (int teamId)
	{
		overdrawModel = Instantiate (thisFactory.model, thisFactory.model.transform.position, thisFactory.model.transform.rotation) as GameObject;
		overdrawModel.transform.parent = thisFactory.model.transform.parent;
		thisFactory.model.SetActive (false);

		foreach (Renderer r in overdrawModel.GetComponentsInChildren<Renderer>())
		{
			foreach (Material m in r.materials)
			{
#if UNITY_ANDROID || UNITY_IPHONE
				m.shader = Shader.Find ("Diffuse");
#endif
				m.color = new Color (0.25f, 0.75f, 0.25f);
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
