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
		thisFactory.InstanceOverdraw();
		correctName = thisFactory.name;
		thisFactory.name = "GhostFactory";		
		ComponentGetter.Get<InteractionController> ().enabled = false;
		ComponentGetter.Get<SelectionController> ().enabled = false;
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		touchController = ComponentGetter.Get<TouchController> ();		
		touchController.DisableDragOn = true;		
		isCapsuleCollider = true;
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
		hcd.Init((other) =>	{OnCollider (other);}, (other) => {OffCollider (other);});		
		SetOverdraw ();
	}
	
	void Update ()
	{
		this.gameObject.layer = LayerMask.NameToLayer ("Gizmos");
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
		if (!other.name.Equals ("Terrain"))	numberOfCollisions++;
	}
	
	void OffCollider (Collider other)
	{
		if (!other.name.Equals ("Terrain"))	numberOfCollisions--;			
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
			thisFactory.ghostFactory = false;
			thisFactory.name = correctName;

			if (GetComponent<NavMeshObstacle> () != null) GetComponent<NavMeshObstacle>().enabled = true;
			
			this.gameObject.layer = LayerMask.NameToLayer ("Unit");
			
			if (!PhotonNetwork.offlineMode)
			{
				GameObject realFactoryObj = PhotonNetwork.Instantiate (factoryConstruction.factory.name, thisFactory.transform.position, thisFactory.transform.rotation, 0);
				thisFactory = realFactoryObj.GetComponent<FactoryBase>();
				thisFactory.photonView.RPC ("Instance", PhotonTargets.All, worker.team, worker.ally);
			}
			else thisFactory.Instance(worker.team, worker.ally);

			gameObject.SendMessage ("OnInstance", SendMessageOptions.DontRequireReceiver);
			StatsController statsController = ComponentGetter.Get<StatsController> ();
			foreach (Unit unit in statsController.selectedStats)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker otherWorker = unit as Worker;
					otherWorker.SetMoveToFactory (thisFactory);
				}
			}

			if (!PhotonNetwork.offlineMode)
			{
				Destroy(gameObject);	
			}
			else
			{
				DestroyOverdrawModel ();			
				Destroy(this);		
			}
		}

		else
		{
			Destroy (gameObject);
		}
	}
	
	void SetOverdraw ()
	{	
		Quaternion factoryRotation = thisFactory.transform.rotation;
		factoryRotation.y = randomRotation;
		overdrawModel = Instantiate (thisFactory.model, thisFactory.model.transform.position, factoryRotation) as GameObject;
		overdrawModel.transform.parent = thisFactory.transform;
		thisFactory.transform.rotation = factoryRotation;
		thisFactory.model.SetActive (false);
		overdrawModel.SetActive(true);
		
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