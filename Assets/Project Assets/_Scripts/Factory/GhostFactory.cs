using UnityEngine;
using System.Collections;
using Visiorama;

public class GhostFactory : MonoBehaviour
{
	protected Worker worker;
	protected TouchController touchController;
	protected FogOfWar fogOfWar;
	protected GameplayManager gameplayManager;

	protected string correctName;
	protected Worker.FactoryConstruction factoryConstruction;
	protected FactoryBase thisFactory;
	protected GameObject overdrawModel;
	protected int numberOfCollisions = 0;
	protected float realRadius;

	public void Init (Worker worker, Worker.FactoryConstruction factoryConstruction)
	{
		GameObject oldGhost = GameObject.Find ("GhostFactory");
		if (oldGhost != null) Destroy (oldGhost);
		
		this.worker 			 = worker;
		this.factoryConstruction = factoryConstruction;

		thisFactory = GetComponent<FactoryBase>();
		thisFactory.photonView.RPC ("InstanceOverdraw", PhotonTargets.All, worker.team);
		
		correctName = thisFactory.name;
		thisFactory.name = "GhostFactory";

		ComponentGetter.Get<InteractionController> ().enabled = false;
		ComponentGetter.Get<SelectionController> ().enabled = false;
		fogOfWar = ComponentGetter.Get<FogOfWar> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		touchController = ComponentGetter.Get<TouchController> ();

		touchController.DisableDragOn = true;

		collider.isTrigger = true;
		realRadius = GetComponent<CapsuleCollider> ().radius;
		GetComponent<CapsuleCollider> ().radius += 3f;
		
		gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;

		SetOverdraw (worker.team);
	}

	void Update ()
	{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if (touchController.idTouch == TouchController.IdTouch.Id1) return;
#endif
		
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
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (numberOfCollisions == 0)
#else
				if (numberOfCollisions == 0 && fogOfWar.IsKnownArea (transform))
#endif
				{
					Apply ();
				}
				else
				{
					Debug.Log (numberOfCollisions);
				}
			}
			else
			{
				ComponentGetter.Get<SelectionController> ().enabled = true;
				ComponentGetter.Get<InteractionController> ().enabled = true;
				PhotonNetwork.Destroy (gameObject);
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
			SetColorOverdraw (new Color (0.75f, 0.25f, 0.25f));
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
			SetColorOverdraw (new Color (0.25f, 0.75f, 0.25f));
		}
	}

	void Apply ()
	{
		ComponentGetter.Get<SelectionController> ().enabled = true;
		ComponentGetter.Get<InteractionController> ().enabled = true;

		bool canBuy = worker.CanConstruct (factoryConstruction);

		if (canBuy)
		{
			GetComponent<CapsuleCollider> ().radius = realRadius;
			
			transform.parent = GameObject.Find("GamePlay/" + gameplayManager.MyTeam).transform;

			collider.isTrigger = false;
			thisFactory.enabled = true;

			thisFactory.name = correctName;
			
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

			Destroy(rigidbody);
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
//				m.shader = Shader.Find ("Overdraw");
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
