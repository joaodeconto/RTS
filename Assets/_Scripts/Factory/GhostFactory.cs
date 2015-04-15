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
	protected float randomRotation;	
	private bool isCapsuleCollider;
	
	private int terrainLayer = (1 << LayerMask.NameToLayer ("Terrain"));
	private int navMeshLayer = (1 << NavMesh.GetNavMeshLayerFromName ("Default"));
	
	public void Init (Worker worker, Worker.FactoryConstruction factoryConstruction)
	{
		GameObject oldGhost = GameObject.Find ("GhostFactory");
		if (oldGhost != null) Destroy (oldGhost);
		randomRotation = Random.rotation.y;		
		this.worker 			 = worker;
		this.factoryConstruction = factoryConstruction;		
		thisFactory = GetComponent<FactoryBase>();
//		if (!PhotonNetwork.offlineMode)thisFactory.photonView.RPC ("InstanceOverdraw", PhotonTargets.All, worker.team, worker.ally);
		thisFactory.InstanceOverdraw(worker.team, worker.ally);
		correctName = thisFactory.name;
		thisFactory.name = "GhostFactory";		
		ComponentGetter.Get<InteractionController> ().enabled = false;
		ComponentGetter.Get<SelectionController> ().enabled = false;
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		touchController = ComponentGetter.Get<TouchController> ();		
		touchController.DisableDragOn = true;		
		isCapsuleCollider = true;		
		thisFactory.gameObject.layer = LayerMask.NameToLayer ("Gizmos");
		if (!PhotonNetwork.offlineMode) GetComponent<PhotonView>().observed = null;	
		GameObject helperColliderGameObject;
		
		if (gameObject.GetComponent<CapsuleCollider> () == null)
		{
			isCapsuleCollider = false;						
			thisFactory.helperCollider.isTrigger = true;
			realRadius = thisFactory.helperCollider.radius;			
			helperColliderGameObject = thisFactory.helperCollider.gameObject;			
			numberOfCollisions--;
		}
		else
		{
			GetComponent<CapsuleCollider> ().isTrigger = true;
			realRadius = GetComponent<CapsuleCollider> ().radius;
			GetComponent<CapsuleCollider> ().radius += 2f;
			
			helperColliderGameObject = gameObject;
		}
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
		if (!touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id1)
		{
			ComponentGetter.Get<SelectionController> ().enabled = true;
			ComponentGetter.Get<InteractionController> ().enabled = true;
			PhotonNetwork.Destroy (gameObject);			
		}
		#endif
		
		Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		NavMeshHit navHit;

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
				if (numberOfCollisions == 0 && collideOnNavMeshLayer == true)Apply ();
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
			gameplayManager.resources.UseResources (factoryConstruction.costOfResources);
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

			if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = true;
			
			if (!PhotonNetwork.offlineMode)
			{
				thisFactory.photonView.RPC ("Instance", PhotonTargets.All);
				FactoryNetworkTransform fnt = GetComponent<FactoryNetworkTransform>();
				GetComponent<PhotonView>().observed = fnt;
			}
			else thisFactory.Instance();
			gameObject.SendMessage ("OnInstance", SendMessageOptions.DontRequireReceiver);
	
			thisFactory.costOfResources = factoryConstruction.costOfResources;
			worker.SetMoveToFactory (thisFactory);
			StatsController statsController = ComponentGetter.Get<StatsController> ();
			foreach (Unit unit in statsController.selectedStats)
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
		Quaternion factoryRotation = thisFactory.model.transform.rotation;
		factoryRotation.y = randomRotation;
		overdrawModel = Instantiate (thisFactory.model, thisFactory.model.transform.position, factoryRotation) as GameObject;
		overdrawModel.transform.parent = thisFactory.transform;
		thisFactory.model.transform.rotation = factoryRotation;
		thisFactory.model.SetActive (false);
		
		foreach (Renderer r in overdrawModel.GetComponentsInChildren<Renderer>())
		{
			foreach (Material m in r.materials)
			{
				m.shader = Shader.Find ("Diffuse");
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