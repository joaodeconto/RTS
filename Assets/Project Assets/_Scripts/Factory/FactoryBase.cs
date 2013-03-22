using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FactoryBase : IStats
{
	public const int MAX_NUMBER_OF_LISTED = 5;
	public const string FactoryQueueName = "Factory";

	[System.Serializable]
	public class UnitFactory
	{
		public Unit unit;
		public ResourcesManager costOfResources;
		public float timeToCreate = 3f;
		public string buttonName;
		public IStats.GridItemAttributes gridItemAttributes;
	}

	[System.Serializable]
	public class BuildingObjects
	{
		public GameObject baseObject;
		public GameObject unfinishedObject;
		public GameObject finishedObject;
	}

	public enum BuildingState
	{
		Base = 0,
		Unfinished = 1,
		Finished = 2
	}

	public UnitFactory[] unitsToCreate;

	public Transform waypoint;

	public Resource.Type receiveResource;

	public BuildingObjects buildingObjects;

	public string guiTextureName;
	public string unitCreatedEventMessage;

	public BuildingState buildingState { get; set; }
	protected int levelConstruct;

	protected List<Unit> listedToCreate = new List<Unit>();
	protected Unit unitToCreate;
	protected float timeToCreate;
	protected float timer;
	protected bool inUpgrade;

	protected bool hasWaypoint;

	public Animation ControllerAnimation { get; private set; }

	public bool wasBuilt { get; private set; }

	protected FactoryController factoryController;
	protected HUDController hudController;
//	protected EventManager eventManager;
	protected HealthBar healthBar;

	public bool wasVisible = false;

	public bool IsNeededRepair
	{
		get {
			return Health != MaxHealth;
		}
	}

	public bool OverLimitCreateUnit
	{
		get
		{
			return listedToCreate.Count >= MAX_NUMBER_OF_LISTED;
		}
	}

	public override void Init ()
	{
		base.Init();

		timer = 0;

		hudController     = ComponentGetter.Get<HUDController> ();
//		eventManager      = ComponentGetter.Get<EventManager> ();
		factoryController = ComponentGetter.Get<FactoryController> ();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();

		if (waypoint == null) waypoint = transform.FindChild("Waypoint");

		hasWaypoint = (waypoint != null);

		if (hasWaypoint) waypoint.gameObject.SetActive (false);

		playerUnit = gameplayManager.IsSameTeam (this);

		this.gameObject.tag = "Factory";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		factoryController.AddFactory (this);

		inUpgrade = false;
		wasBuilt = true;

		buildingState = BuildingState.Finished;

		enabled = playerUnit;
	}

	void Update ()
	{
		SyncAnimation ();

		if (!wasBuilt ||
			listedToCreate.Count == 0) return;

		if (unitToCreate == null)
		{
			unitToCreate = listedToCreate[0];
			foreach (UnitFactory uf in unitsToCreate)
			{
				if (uf.unit == unitToCreate)
				{
					timeToCreate = uf.timeToCreate;
				}
			}
			inUpgrade = true;
		}
		else
		{
			if (timer > timeToCreate)
			{
				InvokeUnit (unitToCreate);

				timer = 0;
				unitToCreate = null;
				inUpgrade = false;
			}
			else
			{
				timer += Time.deltaTime;
			}
		}
	}

	void OnDestroy ()
	{
		if (!IsRemoved && !playerUnit) factoryController.factorys.Remove (this);
	}

	void OnGUI ()
	{
		if (Selected)
		{
			if (inUpgrade)
			{
				GUI.Box(new Rect(Screen.width/2 - 50, Screen.height - 50, 100, 25), "");
				GUI.Box(new Rect(Screen.width/2 - 50, Screen.height - 50, 100 * (timer / timeToCreate), 25), "");
			}
		}
	}

	public virtual void SyncAnimation ()
	{
		if (!IsVisible) return;

		buildingObjects.baseObject.SetActive (buildingState == BuildingState.Base);
		buildingObjects.unfinishedObject.SetActive (buildingState == BuildingState.Unfinished);
		buildingObjects.finishedObject.SetActive (buildingState == BuildingState.Finished);
	}

	void InvokeUnit (Unit unit)
	{
		listedToCreate.RemoveAt (0);

		hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);

//		string unitName = "";
//
//		foreach(UnitFactory uf in unitsToCreate)
//		{
//			if(uf.unit == unit)
//			{
//				unitName = uf.buttonName;
//				break;
//			}
//		}
//
//		eventManager.AddEvent(unitCreatedEventMessage + " " + unitName, unit.guiTextureName);

		if (!hasWaypoint) return;

		// Look At
		Vector3 difference = waypoint.position - transform.position;
		Quaternion rotation = Quaternion.LookRotation (difference);
		Vector3 forward = rotation * Vector3.forward;

		Vector3 unitSpawnPosition = transform.position + (forward * GetComponent<CapsuleCollider>().radius);

		if (PhotonNetwork.offlineMode)
		{
			Unit newUnit = Instantiate (unit, unitSpawnPosition, Quaternion.identity) as Unit;
//			newUnit.Move (transform.position + (transform.forward * GetComponent<CapsuleCollider>().radius) * 2);
			newUnit.Move (waypoint.position);
		}
		else
		{
	        GameObject newUnit = PhotonNetwork.Instantiate(unit.gameObject.name, unitSpawnPosition, Quaternion.identity, 0);
//			newUnit.GetComponent<Unit> ().Move (transform.position + (transform.forward * GetComponent<CapsuleCollider>().radius) * 2);
			newUnit.GetComponent<Unit> ().Move (waypoint.position);
		}
	}

	void OnDie ()
	{
		factoryController.RemoveFactory (this);
		if (IsNetworkInstantiate)
		{
			if (photonView.isMine) PhotonNetwork.Destroy(gameObject);
		}
		else Destroy (gameObject);
	}

	[RPC]
	public void InstanceOverdraw (int teamID)
	{
		levelConstruct = Health = 1;
		wasBuilt = false;
		Team = teamID;
		factoryController.RemoveFactory (GetComponent<FactoryBase> ());

		if (!photonView.isMine) model.SetActive (false);
		if (!PhotonNetwork.offlineMode) IsNetworkInstantiate = true;
	}

	[RPC]
	public void Instance ()
	{
		factoryController.AddFactory (this);
		buildingState = BuildingState.Base;
		if (!gameplayManager.IsSameTeam (Team)) model.SetActive (true);
	}

	public bool Construct (Worker worker)
	{
		if (levelConstruct < (MaxHealth / 2))
		{
			buildingState = BuildingState.Base;
		}
		else if (levelConstruct < MaxHealth)
		{
			buildingState = BuildingState.Unfinished;
		}
		else
		{
			buildingState = BuildingState.Finished;
		}

		if (levelConstruct == MaxHealth)
		{
			if (!wasBuilt)
			{
				wasBuilt = true;
				SendMessage ("ConstructFinished");
			}
			return false;
		}
		else
		{
			levelConstruct += worker.constructionAndRepairForce;
			levelConstruct = Mathf.Clamp (levelConstruct, 0, MaxHealth);
			Health += worker.constructionAndRepairForce;
			Health = Mathf.Clamp (Health, 0, MaxHealth);
			return true;
		}
	}

	public bool Repair (Worker worker)
	{
		if (IsNeededRepair)
		{
			Health += worker.constructionAndRepairForce;
			Health = Mathf.Clamp (Health, 0, MaxHealth);
			return true;
		}
		else
		{
			return false;
		}
	}

	public void Select ()
	{
		if (!Selected) Selected = true;
		else return;

		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (Team));

		if (playerUnit && wasBuilt)
		{
			if (!hasWaypoint) return;

			waypoint.gameObject.SetActive (true);
			if (!waypoint.gameObject.activeSelf)
				waypoint.gameObject.SetActive (true);

			foreach (UnitFactory uf in unitsToCreate)
			{
				Hashtable ht = new Hashtable();
				ht["unit"]    = uf.unit;

				hudController.CreateButtonInInspector ( uf.buttonName,
														uf.gridItemAttributes.Position,
														ht,
														uf.unit.guiTextureName,
														(ht_hud) =>
														{
															FactoryBase factory = this;
															Unit unit           = (Unit)ht_hud["unit"];

															if (!factory.OverLimitCreateUnit)
																factory.EnqueueUnitToCreate (unit);
														});
			}

			for(int i = listedToCreate.Count - 1; i != -1; --i)
			{
				Unit unit = listedToCreate[i];

				Hashtable ht = new Hashtable();
				ht["unit"] = listedToCreate[i];
				ht["name"] = "button-" + Time.time;

				hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
																FactoryBase.FactoryQueueName,
																ht,
																listedToCreate[i].guiTextureName,
																(hud_ht) =>
																{
																	//TODO Fazer recuperação do dinheiro quando desistir da
																	//construção de alguma unidade

																	DequeueUnit(hud_ht);
																});
			}
		}
	}

	public bool Deselect (bool isGroupDelesection = false)
	{
		if (Selected) Selected = false;
		else return false;

		hudController.DestroySelected (transform);

		if (playerUnit && wasBuilt)
		{
			if (!hasWaypoint) return true;

			waypoint.gameObject.SetActive (false);

			if(!isGroupDelesection)
			{
				hudController.DestroyInspector ();
			}
		}

		return true;
	}

	public void EnqueueUnitToCreate (Unit unit)
	{
		bool canBuy = true;
		foreach (UnitFactory uf in unitsToCreate)
		{
			if (unit == uf.unit)
			{
				canBuy = gameplayManager.resources.CanBuy (uf.costOfResources);
				break;
			}
		}

		if (canBuy)
		{
			listedToCreate.Add (unit);
			Hashtable ht = new Hashtable();
			ht["unit"] = unit;
			ht["name"] = "button-" + Time.time;

			//TODO colocar mais coisas aqui
			hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
															FactoryBase.FactoryQueueName,
															ht,
															unit.guiTextureName,
															(hud_ht) =>
															{
																//TODO cancelar construnção do item
																DequeueUnit(hud_ht);
															});
		}
		else
			;//TODO mensagem para o usuário saber
			 //que não possui recursos suficientes para criar tal unidade
	}

	private void DequeueUnit(Hashtable ht)
	{
		string btnName = (string)ht["name"];
		Unit unit = (Unit)ht["unit"];

		Debug.Log("btnName: " + btnName);

		if(hudController.CheckQueuedButtonIsFirst(btnName, FactoryBase.FactoryQueueName))
		{
			Debug.Log("chegouvids");
			timer = 0;
			unitToCreate = null;
			inUpgrade = false;
		}

		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
		listedToCreate.Remove (unit);
	}

	public override void SetVisible(bool isVisible)
	{
		ComponentGetter.Get<FactoryController> ().ChangeVisibility (this, isVisible);

		if(isVisible)
		{
			model.transform.parent = this.transform;

			if(!wasVisible)
			{
				wasVisible = true;
				model.SetActive(true);
			}
		}
		else
		{
			model.transform.parent = null;

			if(!wasVisible)
				model.SetActive(false);
		}
	}

	public override bool IsVisible
	{
		get
		{
			return model.transform.parent != null;
		}
	}
}
