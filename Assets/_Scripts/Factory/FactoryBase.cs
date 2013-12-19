using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;
using Visiorama.Extension;

public class FactoryBase : IStats
{
	public const int MAX_NUMBER_OF_LISTED = 5;
	public const string FactoryQueueName = "Factory";

	[System.Serializable]
	public class UnitFactory
	{
		public Unit unit;
		public string buttonName;
		public IStats.GridItemAttributes gridItemAttributes;
	}

	[System.Serializable]
	public class BuildingObjects
	{
		public GameObject baseObject;
		public GameObject unfinishedObject;
		public GameObject finishedObject;
		public GameObject[] desactiveObjectsWhenInstance;
	}

	public enum BuildingState
	{
		Base       = 0,
		Unfinished = 1,
		Finished   = 2
	}

	public UnitFactory[] unitsToCreate;

	public Resource.Type receiveResource;

	public BuildingObjects buildingObjects;

	public string buttonName;
	public string guiTextureName;
	
	public CapsuleCollider helperCollider { get; private set; }

	public bool hasRallypoint { get; private set; }
	
	private Transform rallypoint;
	
	public BuildingState buildingState { get; set; }
	protected int levelConstruct;

	protected List<Unit> listedToCreate = new List<Unit>();
	protected Unit unitToCreate;
	protected float timeToCreate;
	protected float timer;
	protected bool inUpgrade;

	public Animation ControllerAnimation { get; private set; }

	public bool wasBuilt { get; private set; }

	protected HUDController hudController;
	protected HealthBar healthBar;
	protected UISlider buildingSlider;

	[HideInInspector]
	public bool wasVisible = false;
	[HideInInspector]
	public bool alreadyCheckedMaxPopulation = false;
//	[HideInInspector]
//	public ResourcesManager costOfResources;

	protected float realRangeView;

	public bool IsNeededRepair
	{
		get
		{
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
		buildingSlider    = hudController.GetSlider("Building Unit");
		buildingSlider.gameObject.SetActive(false);

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();
		
		helperCollider = GetComponentInChildren<CapsuleCollider> ();

		if (unitsToCreate.Length != 0)
		{
			hasRallypoint = true;
			
			GameObject instantiateRallypoint = Resources.Load ("Rallypoint", typeof(GameObject)) as GameObject;
			
			GameObject goRallypoint = NGUITools.AddChild (gameObject, instantiateRallypoint);

			rallypoint = goRallypoint.transform;
			rallypoint.parent = this.transform;

			Vector3 pos = rallypoint.position;
			pos.z -= transform.collider.bounds.size.z;
			rallypoint.position = pos;

			rallypoint.gameObject.SetActive (false);
			
			hasRallypoint = (rallypoint != null);
		}

		playerUnit = gameplayManager.IsSameTeam (this);

		this.gameObject.tag   = "Factory";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		inUpgrade = false;
		wasBuilt  = true;

		buildingState = BuildingState.Finished;

		enabled = playerUnit;

		Invoke ("SendMessageInstance", 0.1f);
	}

	void SendMessageInstance ()
	{
		if (GetComponent<GhostFactory> () == null)
			SendMessage ("OnInstanceFactory", SendMessageOptions.DontRequireReceiver);
	}

	void Update ()
	{
		SyncAnimation ();

		if (!wasBuilt || listedToCreate.Count == 0)
			return;

		if (gameplayManager.NeedMoreHouses (listedToCreate[0].numberOfUnits))
		{
			if (!alreadyCheckedMaxPopulation)
			{
				alreadyCheckedMaxPopulation = true;
				eventManager.AddEvent("need more houses");
			}
			return;
		}
		else
			alreadyCheckedMaxPopulation = false;

		if (unitToCreate == null)
		{
			unitToCreate = listedToCreate[0];
			timeToCreate = listedToCreate[0].timeToSpawn;
			inUpgrade = true;

			if (Selected) buildingSlider.gameObject.SetActive(true);
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
				buildingSlider.sliderValue = (timer / timeToCreate);
			}
		}
	}

	void OnDestroy ()
	{
		if (!IsRemoved && !playerUnit) statsController.RemoveStats (this);
	}

	//void OnGUI ()
	//{
		//if (Selected)
		//{
			//if (inUpgrade)
			//{
				//GUI.Box(new Rect(Screen.width/2 - 50, Screen.height - 50, 100, 25), "");
				//GUI.Box(new Rect(Screen.width/2 - 50, Screen.height - 50, 100 * (timer / timeToCreate), 25), "");
			//}
		//}
	//}

	public virtual void SyncAnimation ()
	{
		if (!IsVisible) return;

		buildingObjects.baseObject.SetActive (buildingState == BuildingState.Base);
		buildingObjects.unfinishedObject.SetActive (buildingState == BuildingState.Unfinished);
		buildingObjects.finishedObject.SetActive (buildingState == BuildingState.Finished);
	}

	void InvokeUnit (Unit unit)
	{
		buildingSlider.gameObject.SetActive(false);

		listedToCreate.RemoveAt (0);

		hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);

		string unitName = "";
		
		foreach(UnitFactory uf in unitsToCreate)
		{
			if(uf.unit == unit)
			{
				unitName = uf.buttonName;
				break;
			}
		}

		if(string.IsNullOrEmpty(unitName))
		{
			Debug.LogError("Eh necessario colocar um nome no UnitFactory.\nUtilizando nome padrao");
			unitName = unit.name;
		}

		eventManager.AddEvent("create unit", unitName, unit.guiTextureName);

		PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
		Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));

		Debug.Log ("battle: " + battle);
		//Score
		Score.AddScorePoints ("Units created", 1);
		Score.AddScorePoints ("Units created", 1, battle.IdBattle);
		Score.AddScorePoints (unitName + " created", 1);
		Score.AddScorePoints (unitName + " created", 1, battle.IdBattle);

		if (!hasRallypoint) return;

		// Look At
		Vector3 difference = rallypoint.position - transform.position;
		Quaternion rotation = Quaternion.LookRotation (difference);
		Vector3 forward = rotation * Vector3.forward;

		Vector3 unitSpawnPosition = transform.position + (forward * helperCollider.radius);

		if (PhotonNetwork.offlineMode)
		{
			Unit newUnit = Instantiate (unit, unitSpawnPosition, Quaternion.identity) as Unit;
//			newUnit.Move (transform.position + (transform.forward * GetComponent<CapsuleCollider>().radius) * 2);
			newUnit.Move (rallypoint.position);
			newUnit.transform.parent = GameObject.Find("GamePlay/" + gameplayManager.MyTeam).transform;
		}
		else
		{
	        GameObject newUnit = PhotonNetwork.Instantiate(unit.gameObject.name, unitSpawnPosition, Quaternion.identity, 0);
//			newUnit.GetComponent<Unit> ().Move (transform.position + (transform.forward * GetComponent<CapsuleCollider>().radius) * 2);
			newUnit.GetComponent<Unit> ().Move (rallypoint.position);
			newUnit.transform.parent = GameObject.Find("GamePlay/" + gameplayManager.MyTeam).transform;
		}
	}

	public virtual IEnumerator OnDie ()
	{
		statsController.RemoveStats (this);

		model.animation.Play ();

		if (Selected)
		{
			hudController.DestroyInspector ("factory");

			Deselect ();
		}

//		yield return StartCoroutine (model.animation.WaitForAnimation (model.animation.clip));

		yield return new WaitForSeconds (4f);
		if (IsNetworkInstantiate)
		{
			PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
			Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));

			if (photonView.isMine)
			{
				PhotonNetwork.Destroy(gameObject);

				Score.AddScorePoints ("Buildings lost", 1);
				Score.AddScorePoints ("Buildings lost", 1, battle.IdBattle);
				Score.AddScorePoints (this.category + " lost", 1);
				Score.AddScorePoints (this.category + " lost", 1, battle.IdBattle);
			}
			else
			{
				Score.AddScorePoints ("Destroyed buildings", 1);
				Score.AddScorePoints ("Destroyed buildings", 1, battle.IdBattle);
				Score.AddScorePoints (this.category + " destroyed", 1);
				Score.AddScorePoints (this.category + " destroyed", 1, battle.IdBattle);
			}
		}
		else Destroy (gameObject);
	}

	[RPC]
	public void InstanceOverdraw (int teamID)
	{
		levelConstruct = Health = 1;
		wasBuilt = false;
		team = teamID;
		statsController.RemoveStats (GetComponent<FactoryBase> ());

		GetComponent<NavMeshObstacle> ().enabled = false;

		foreach (GameObject obj in buildingObjects.desactiveObjectsWhenInstance)
		{
			obj.SetActive (false);
		}

		if (!photonView.isMine) model.SetActive (false);
		if (!PhotonNetwork.offlineMode) IsNetworkInstantiate = true;

// =================================================================
// |                                                               |
// |      UTILIZANDO COMPONENTE FOW EM TODAS ESTRUTURAS!!!         |
// |                 DEVE SER ATUALIZADO!!!                        |
// |                                                               |
// =================================================================
//		if (gameplayManager.SameEntity (team, ally))
//		{
//			FOWRevealer fowr = gameObject.GetComponent<FOWRevealer>();
//			fowr.range = new Vector2(0, 0);
//		}
	}

	[RPC]
	public void Instance ()
	{
		realRangeView  = this.fieldOfView;
//		if (gameplayManager.SameEntity (team, ally))
//		{
//			FOWRevealer fowr = gameObject.GetComponent<FOWRevealer>();
//			fowr.range = new Vector2(0, helperCollider.radius);
//		}

		GetComponent<NavMeshObstacle> ().enabled = true;

		statsController.AddStats (this);
		foreach (GameObject obj in buildingObjects.desactiveObjectsWhenInstance)
		{
			obj.SetActive (true);
		}

		buildingState = BuildingState.Base;

		SendMessage ("OnInstanceFactory", SendMessageOptions.DontRequireReceiver);

		if (!gameplayManager.IsSameTeam (team)) model.SetActive (true);
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

				this.fieldOfView = realRangeView;

				string factoryName = buttonName;

				if(string.IsNullOrEmpty(factoryName))
				{
					Debug.LogError("Eh necessario colocar um nome no buttonName.\nUtilizando nome padrao");
					factoryName = this.name;
				}

				eventManager.AddEvent("building finish", factoryName, this.guiTextureName);
				SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);

				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));

				Score.AddScorePoints ("Buildings created", 1);
				Score.AddScorePoints ("Buildings created", 1, battle.IdBattle);
				Score.AddScorePoints (factoryName + " created", 1);
				Score.AddScorePoints (factoryName + " created", 1, battle.IdBattle);
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

	public override void Select ()
	{
		base.Select ();

		if(unitToCreate != null)
			buildingSlider.gameObject.SetActiveRecursively(true);

		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));

		if (!playerUnit) return;

		if (wasBuilt)
		{
			if (!hasRallypoint) return;

			rallypoint.gameObject.SetActive (true);
			if (!rallypoint.gameObject.activeSelf)
				rallypoint.gameObject.SetActive (true);

			foreach (UnitFactory uf in unitsToCreate)
			{
				Hashtable ht = new Hashtable();
				ht["unitFactory"] = uf;
				ht["price"]       = uf.unit.costOfResources.NumberOfRocks;

				hudController.CreateButtonInInspector ( uf.buttonName,
														uf.gridItemAttributes.Position,
														ht,
														uf.unit.guiTextureName,
														(ht_hud) =>
														{
															List<FactoryBase> factorys = new List<FactoryBase> ();
															UnitFactory unitFactory = (UnitFactory)ht_hud["unitFactory"];

															foreach (IStats stat in statsController.selectedStats)
															{
																FactoryBase factory = stat as FactoryBase;

																if (factory == null) continue;

																factorys.Add (factory);
															}

															int i = 0, factoryChoose = 0, numberToCreate = -1;

															foreach (FactoryBase factory in factorys)
															{
																if (numberToCreate == -1)
																{
																	numberToCreate = factory.listedToCreate.Count;
																	factoryChoose = i;
																}
																else if (numberToCreate > factory.listedToCreate.Count)
																{
																	numberToCreate = factory.listedToCreate.Count;
																	factoryChoose = i;
																}
																i++;
															}

															if (!factorys[factoryChoose].OverLimitCreateUnit)
																factorys[factoryChoose].EnqueueUnitToCreate (unitFactory.unit);
															else
																eventManager.AddEvent("reach enqueued units");


//															FactoryBase factory     = this;
//															UnitFactory unitFactory = (UnitFactory)ht_hud["unitFactory"];
//
//															if (!factory.OverLimitCreateUnit)
//																factory.EnqueueUnitToCreate (unitFactory.unit);
//															else
//																eventManager.AddEvent("reach enqueued units");
														});
			}

			for(int i = 0; i != listedToCreate.Count; ++i)
			{
				Unit unit = listedToCreate[i];

				Hashtable ht = new Hashtable ();
				ht["unit"] = unit;
				ht["name"] = "button-" + Time.time;

				hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
																FactoryBase.FactoryQueueName,
																ht,
																listedToCreate[i].guiTextureName,
																(hud_ht) =>
																{
																	DequeueUnit(hud_ht);
																});
			}
		}
		else
		{
			IStats.GridItemAttributes gia = new GridItemAttributes();
			gia.gridXIndex = 0;
			gia.gridYIndex = 0;

			Hashtable ht = new Hashtable();
			ht["actionType"] = "cancel";

			hudController.CreateButtonInInspector ( "cancel",
													gia.Position,
													ht,
													"cross-option",
													(ht_hud) =>
													{
														gameplayManager.resources.ReturnResources (costOfResources, 0.75f);
														photonView.RPC ("SendRemove", PhotonTargets.All);
													});
		}
	}

	public override void Deselect ()
	{
		base.Deselect ();

		buildingSlider.gameObject.SetActive(false);

		hudController.DestroySelected (transform);

		if (playerUnit && wasBuilt)
		{
			if (!hasRallypoint) return;

			rallypoint.gameObject.SetActive (false);

			hudController.DestroyInspector ("factory");
		}
	}

	public void EnqueueUnitToCreate (Unit unit)
	{
		bool canBuy = gameplayManager.resources.CanBuy (unit.costOfResources);

		if (canBuy)
		{
			listedToCreate.Add (unit);
			Hashtable ht = new Hashtable();
			ht["unit"] = unit;
			ht["name"] = "button-" + Time.time;

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
			eventManager.AddEvent("out of founds", unit.name);
	}

	private void DequeueUnit(Hashtable ht)
	{
		string btnName = (string)ht["name"];
		Unit unit = 	 (Unit)ht["unit"];

		gameplayManager.resources.ReturnResources (unit.costOfResources);

		if(hudController.CheckQueuedButtonIsFirst(btnName, FactoryBase.FactoryQueueName))
		{
			timer = 0;
			unitToCreate = null;
			inUpgrade = false;
		}

		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
		listedToCreate.Remove (unit);
	}
	
	Unit CheckUnit (Unit unit)
	{
		foreach (UnitFactory uf in unitsToCreate)
		{
			if (unit == uf.unit)
			{
				return uf.unit;
			}
		}
		
		return null;
	}

	public override void SetVisible(bool isVisible)
	{
		statsController.ChangeVisibility (this, isVisible);

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

	// RPCs
	[RPC]
	public override void InstantiatParticleDamage ()
	{
		base.InstantiatParticleDamage ();
	}

	[RPC]
	public override void SendRemove ()
	{
		base.SendRemove ();
	}
}
