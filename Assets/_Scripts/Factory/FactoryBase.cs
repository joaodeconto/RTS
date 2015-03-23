using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Extension;

public class FactoryBase : IStats, IDeathObservable
{
	#region Serializable
	
	[System.Serializable]
	public class UnitFactory
	{
		public Unit unit;
		public string buttonName;
		public bool VIP = false;
		public bool techAvailable = false;
		public IStats.GridItemAttributes gridItemAttributes;
		//		public ResourcesManager costOfResources;
	}
	
	[System.Serializable]
	public class UpgradeItem
	{
		public Upgrade upgrade;
		public string buttonName;
		public bool techAvailable = false;
		public bool VIP = false;
		public bool isChildtech = false;
		public Upgrade childTech;
		public IStats.GridItemAttributes gridItemAttributes;
		public bool alreadyUpgraded = false;
		//		public ResourcesManager costOfResources;
	}
	
	[System.Serializable]
	public class BuildingObjects
	{
		public GameObject baseObject;
		public GameObject unfinishedObject;
		public GameObject finishedObject;
		public GameObject upgradedObject;
		public GameObject[] desactiveObjectsWhenInstance;
	}

	#endregion

	#region Declare
	public const int MAX_NUMBER_OF_LISTED = 5;
	public const string FactoryQueueName = "Factory";
	public UnitFactory[] unitsToCreate;
	public UpgradeItem[] upgradesToCreate;
	public List<int> keys = new List<int>();
	public Resource.Type receiveResource;
	public BuildingObjects buildingObjects;
	public string buttonName;
	public string guiTextureName;	
	public CapsuleCollider helperCollider { get; set; }	
	public bool hasRallypoint { get; set; }	
	public Transform goRallypoint;	
	public BuildingState buildingState { get; set; }
	protected int levelConstruct;
	protected Hashtable invokeQueue = new Hashtable();
	protected List<Unit> lUnitsToCreate = new List<Unit>();
	protected List<Upgrade> lUpgradesToCreate = new List<Upgrade>();
	protected Unit unitToCreate;
	protected Upgrade upgradeToCreate;
	public float timeToCreate;
	public float timer;	
	public float boostDuration = 30;
	public ResourcesManager boostCost;
	[HideInInspector]
	public float boostPower = 1.3f;
	private float boostTime = 0;
	private UILabel boostTimeLabel;
	private bool onBoost;
	private bool canBoost;	
	public bool inUpgrade {get;set;}
	public List<string> TechsToActive = new List<string>();	
	public Animation ControllerAnimation { get; private set; }	
	public bool wasBuilt { get; set; }	
	protected HealthBar healthBar;
	protected UISlider buildingSlider;	
	protected bool isInvokingSlider{get; set;}
	private int queueTicket			 = 0;
	private int queueCounter	 	 = 0;
	private int checkPopCouter	 	 = 0;
	
	[HideInInspector]
	public bool wasVisible = false;
	private bool alreadyCheckedMaxPopulation	{get { return checkPopCouter > 3;}}
	private bool needHouse = false;
	protected float realRangeView;	
	public bool IsDamaged {	get	{ return Health != MaxHealth;}}
	public bool ReachedMaxEnqueued	{get { return EnqueuedCount  >= MAX_NUMBER_OF_LISTED;}}	
	private int EnqueuedCount {get {	return invokeQueue.Count;}}
	public enum BuildingState
	{
		Base       = 0,
		Unfinished = 1,
		Finished   = 2,
		Upgraded   = 3
	}
		
	List<IDeathObserver> IDOobservers = new List<IDeathObserver> ();

	#endregion

	#region Init
	
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
			
			instantiateRallypoint = NGUITools.AddChild (gameObject, instantiateRallypoint);
			
			goRallypoint = instantiateRallypoint.transform;
			goRallypoint.parent = this.transform;
			goRallypoint.gameObject.SetActive (false);
			
			Vector3 pos = goRallypoint.position;
			pos.z -= transform.collider.bounds.size.z;
			
			RallyPoint rallyPoint = goRallypoint.GetComponent<RallyPoint> ();
			rallyPoint.Init (pos, this.team);
		}
		
		playerUnit = gameplayManager.IsSameTeam (this);
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");		
		inUpgrade = false;				
		buildingState = BuildingState.Finished;
		if (playerUnit)
		{
			if (techTreeController.attribsHash.ContainsKey(category))LoadStandardAttribs();
		    if(wasBuilt)TechActiveBool(TechsToActive, true);
		}
		wasBuilt = true;
		enabled = playerUnit;
		Invoke ("SendMessageInstance", 0.1f);
						
	}
	#endregion

	#region Update
	void Update ()
	{
		SyncAnimation ();
				
		if (!wasBuilt)	return;

		if (EnqueuedCount == 0) return;

		if (!inUpgrade)
		{
			if ((upgradeToCreate == null && unitToCreate == null) || needHouse)
			{
				upgradeToCreate = invokeQueue[queueCounter] as Upgrade;
				unitToCreate = invokeQueue[queueCounter] as Unit;

				if (upgradeToCreate is Upgrade)
				{
					timeToCreate = upgradeToCreate.timeToSpawn;
					unitToCreate = null;
				}

				else if (unitToCreate is Unit)
				{
					if (gameplayManager.NeedMoreHouses (unitToCreate.numberOfUnits))
					{
						if (!alreadyCheckedMaxPopulation)
						{
							checkPopCouter++;
							eventController.AddEvent("need more houses", transform.position);
						}
						needHouse = true;
						return ;
					}
					
					else
					{
						needHouse = false;
						checkPopCouter = 0;				
						timeToCreate = unitToCreate.timeToSpawn;
						upgradeToCreate = null;
					}
				}

				else
				{
					Debug.LogWarning("Pulou a fila  " + queueCounter);
					queueCounter++;   // passa um na fila, supostamente dequeued				
					return;
				}

				inUpgrade = true;

				if (Selected)
				{
					buildingSlider.gameObject.SetActive(true);
					InvokeRepeating ("InvokeSliderUpdate",0.2f,0.2f);
				}

				hudController.CreateSubstanceResourceBar (this, sizeOfResourceBar, timeToCreate);					
			}
		}		                           
			
		else		
		{
			if(!Selected && isInvokingSlider)
			{
				CancelInvokeSlider();
			}

			if (timer > timeToCreate)
			{
				CancelInvokeSlider();

				if (unitToCreate != null) 
				{
					InvokeUnit (unitToCreate);
					unitToCreate = null;
				}

				if (upgradeToCreate != null) 
				{
					InvokeUpgrade (upgradeToCreate);
					upgradeToCreate = null;
				}

				invokeQueue.Remove(queueCounter);
				inUpgrade = false;
				timer = 0;
				queueCounter++;
			}
			else
			{
				timer += Time.deltaTime;	
				if(Selected && !isInvokingSlider) InvokeRepeating ("InvokeSliderUpdate",0.2f,0.2f);
			}
		}
	}

	#endregion

	#region Only About the Structure

	public override void SetVisible(bool isVisible)
	{		
		statsController.ChangeVisibility (this, isVisible);
		
		if(isVisible)
		{
			model.transform.parent = this.transform;
			model.SetActive(true);
			if (firstDamage)
			{
				hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
			}
			
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

	void SendMessageInstance ()
	{
		if (GetComponent<GhostFactory> () == null)
			SendMessage ("OnInstanceFactory", SendMessageOptions.DontRequireReceiver);
	}
	
	void OnDestroy ()
	{
		if (!WasRemoved && !playerUnit) statsController.RemoveStats (this);
	}	
		
	public virtual void SyncAnimation ()
	{
		if (!IsVisible) return;
		
		buildingObjects.baseObject.SetActive (buildingState == BuildingState.Base);
		buildingObjects.unfinishedObject.SetActive (buildingState == BuildingState.Unfinished);
		buildingObjects.finishedObject.SetActive (buildingState == BuildingState.Finished);

		if (buildingObjects.upgradedObject != null)
		buildingObjects.upgradedObject.SetActive (buildingState == BuildingState.Upgraded);
	}
			
	public virtual IEnumerator OnDie ()
	{
		statsController.RemoveStats (this);
		inUpgrade = false;
		if (playerUnit && wasBuilt)TechActiveBool(TechsToActive, false);		
		model.animation.Play ();
		
		if (Selected)
		{
			hudController.DestroyInspector ("factory");
			hudController.DestroyOptionsBtns();
			Deselect ();
		}		
		//IDeathObservable
		NotifyDeath ();		
		int c = IDOobservers.Count;
		while (--c != -1)
		{
			UnRegisterDeathObserver (IDOobservers[c]);
		}		
			
		yield return new WaitForSeconds (2f);
		if (IsNetworkInstantiate)
		{
			PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
			Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));
			
			if (photonView.isMine && !gameplayManager.IsBotTeam (this))
			{
				PhotonNetwork.Destroy(gameObject);
				
				Score.AddScorePoints (DataScoreEnum.BuildingsLost, 1);
				Score.AddScorePoints (DataScoreEnum.BuildingsLost, 1, battle.IdBattle);
				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1, battle.IdBattle);
			}

			else
			{
				if(gameplayManager.IsBotTeam (this)) PhotonNetwork.Destroy(gameObject);

				Score.AddScorePoints (DataScoreEnum.DestroyedBuildings, 1);
				Score.AddScorePoints (DataScoreEnum.DestroyedBuildings, 1, battle.IdBattle);
				Score.AddScorePoints (this.category + DataScoreEnum.XDestroyed, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XDestroyed, 1, battle.IdBattle);
			}
		}
		else Destroy (gameObject);
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
					factoryName = this.name;
				}
				
				Init ();
				
				eventController.AddEvent("building finish",transformParticleDamageReference.position, factoryName, this.guiTextureName);
				SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);				
				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));
				
				Score.AddScorePoints (DataScoreEnum.BuildingsCreated, 1);
				Score.AddScorePoints (DataScoreEnum.BuildingsCreated, 1, battle.IdBattle);
				Score.AddScorePoints (factoryName + DataScoreEnum.XCreated, 1);
				Score.AddScorePoints (factoryName + DataScoreEnum.XCreated, 1, battle.IdBattle);
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
		if (IsDamaged)
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

	#endregion
	
	#region Select

	public override void Select ()
	{
		base.Select ();		

		hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));
		
		if (!playerUnit) return;
		
		if (wasBuilt)
		{	
			RestoreOptionsMenu(); //chama os options de upgrade e unit
			RestoreDequeueMenu();
			buildingSlider.gameObject.SetActive(true);	
			if (!hasRallypoint) return;			
			if (!goRallypoint.gameObject.activeSelf) goRallypoint.gameObject.SetActive (true);						
	
		}

		else
		{
			IStats.GridItemAttributes gia = new GridItemAttributes();
			gia.gridXIndex = 3;
			gia.gridYIndex = 2;
			
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

	public void CreateUnitOption(UnitFactory uf)
	{		
		if(!uf.techAvailable)
		{
			Hashtable ht = new Hashtable();
			ht["buttonType"] = uf;
			ht["disable"] = 0;
			
			hudController.CreateButtonInInspector (uf.buttonName,
			                                       uf.gridItemAttributes.Position,
			                                       ht,
			                                       uf.unit.guiTextureName,
			                                       null,
			                                       (ht_dcb, onClick) => 
			                                       {
														UnitFactory unitFactory = (UnitFactory)ht_dcb["buttonType"];
														
														hudController.OpenInfoBoxUnit(unitFactory.unit,true);
													});							
			
		}
		
		else
		{
			Hashtable ht = new Hashtable();
			ht["buttonType"] = uf;
			ht["time"] = 0f;					
			
			if (uf.unit.costOfResources.Rocks != 0)	ht["gold"] = uf.unit.costOfResources.Rocks;					
			if (uf.unit.costOfResources.Mana != 0)	ht["mana"] = uf.unit.costOfResources.Mana;					
			
			hudController.CreateButtonInInspector (uf.buttonName,
			                                       uf.gridItemAttributes.Position,
			                                       ht,
			                                       uf.unit.guiTextureName,
			                                       null,
			                                       (ht_dcb, isDown) => 
			                                       {
														UnitFactory unitFactory = (UnitFactory)ht_dcb["buttonType"];
														
														if (isDown)
														{
															ht["time"] = Time.time;
														}
														else
														{
															if (Time.time - (float)ht["time"] > 0.5f)
															{	
																hudController.OpenInfoBoxUnit(unitFactory.unit, false);																		
															}
															else
															{						
																bool canBuy = gameplayManager.resources.CanBuy (unitFactory.unit.costOfResources);
																
																if (canBuy)
																{
																	FactoryBase	factoryChoose = CheckAvailableFactory();
																	if(factoryChoose == null) return;
																	hudController.CloseInfoBox();
																	factoryChoose.EnqueueUnitToCreate (unitFactory.unit);							
																}
																
																else
																	eventController.AddEvent("out of funds", unitFactory.unit.category);																

															}
														}
													});
		}		
	}
	
	public void CreateUpgradeOption(UpgradeItem ui)
	{
		if(ui.isChildtech)return;
		
		if(!ui.techAvailable)
		{
			Hashtable ht = new Hashtable();
			ht["buttonType"] = ui;
			ht["disable"] = 0;
			
			hudController.CreateButtonInInspector (ui.buttonName,
			                                       ui.gridItemAttributes.Position,
			                                       ht,
			                                       ui.upgrade.guiTextureName,
			                                       null,
			                                       (ht_dcb, onClick) => 
			                                       {
														UpgradeItem upgrade = (UpgradeItem)ht_dcb["buttonType"];																
														hudController.OpenInfoBoxUpgrade(upgrade.upgrade, false);
													});
		}
		
		else
		{
			Hashtable ht = new Hashtable();
			ht["buttonType"] = ui;
			ht["time"] = 0f;
			
			if (ui.upgrade.costOfResources.Rocks != 0)	ht["gold"] = ui.upgrade.costOfResources.Rocks;			
			if (ui.upgrade.costOfResources.Mana != 0)	ht["mana"] = ui.upgrade.costOfResources.Mana;
			
			if (!ui.alreadyUpgraded && !ui.upgrade.uniquelyUpgraded)
			{										
				hudController.CreateButtonInInspector (ui.buttonName,
				                                       ui.gridItemAttributes.Position,
				                                       ht,
				                                       ui.upgrade.guiTextureName,
				                                       null,
				                                       (ht_dcb, isDown) =>				                                       
				                                       {
															UpgradeItem upgrade = (UpgradeItem)ht_dcb["buttonType"];

															if (isDown) ht["time"] = Time.time;

															else
															{
																if (Time.time - (float)ht["time"] >  0.5f)	hudController.OpenInfoBoxUpgrade(upgrade.upgrade, true);																	
																
																else
																{
																	bool canBuy = gameplayManager.resources.CanBuy (upgrade.upgrade.costOfResources);
																	
																	if (canBuy)
																	{
																		FactoryBase	factoryChoose = CheckAvailableFactory();
																		if(factoryChoose == null) return;	
																		hudController.CloseInfoBox();		
																		factoryChoose.EnqueueUpgradeToCreate (upgrade.upgrade);	
																		hudController.RemoveButtonInInspector (upgrade.buttonName);
																		upgrade.alreadyUpgraded = true;	
																		
																		if(upgrade.childTech != null)
																		{ 
																			techTreeController.UpgradeChildBoolOperator(upgrade.childTech.upgradeName, false);									                                         
																		}																						
																									
																	}
																	
																	else
																		eventController.AddEvent("out of funds", upgrade.upgrade.name);
																}	
															}
														});
			}
		}
	}
		
	public override void Deselect ()
	{
		base.Deselect ();		
		buildingSlider.gameObject.SetActive(false);		
		int c = IDOobservers.Count;
		while (--c != -1)
		{
			UnRegisterDeathObserver (IDOobservers[c]);
		}
		
		if (playerUnit && wasBuilt)
		{
			if (!hasRallypoint) return;			
			goRallypoint.gameObject.SetActive (false);	
		
		}
	}
	
	private void RestoreOptionsMenu()
	{
		hudController.DestroyOptionsBtns();
		
		foreach (UpgradeItem ui in upgradesToCreate)
		{
			CreateUpgradeOption(ui);
		}
		foreach (UnitFactory uf in unitsToCreate)
		{
			CreateUnitOption(uf);
		}
	}

	private void RestoreDequeueMenu()
	{
		hudController.DestroyInspector("factory");
		keys.Clear();

		foreach (int key in invokeQueue.Keys)
		{
			keys.Add(key);
		}
		keys.Sort();
		
		foreach (int key in keys)
		{
			Unit unit = invokeQueue[key] as Unit;
			Upgrade upgrade = invokeQueue[key] as Upgrade;
			Hashtable ht = new Hashtable ();
			
			if (unit != null) { ht["buttonType"] = unit; ht["guiTexture"] = unit.guiTextureName;}
			else if (upgrade != null) { ht["buttonType"] = upgrade; ht["guiTexture"] = upgrade.guiTextureName;}
			
			ht["name"] = "button-" + key;
			ht["queueSpot"] = key;
			
			hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
			                                               FactoryBase.FactoryQueueName,
			                                               ht,
			                                               (string)ht["guiTexture"],
			                                               (hud_ht) =>
			                                               {
																if (unit != null) DequeueUnit(hud_ht);
																else if (upgrade != null) DequeueUpgrade(hud_ht);
																RestoreDequeueMenu();
																
															});			
			
		}
	}		
		#endregion
				
		#region FactoryQueue
		
		public void EnqueueUpgradeToCreate (Upgrade upgrade)
	{
			gameplayManager.resources.UseResources (upgrade.costOfResources);
			invokeQueue.Add(queueTicket, upgrade);			Hashtable ht = new Hashtable();
			ht["buttonType"] = upgrade;
			ht["name"] = "button-" + queueTicket;
			ht["queueSpot"] = queueTicket;
			queueTicket++;
			
			hudController.CreateEnqueuedButtonInInspector ((string)ht["name"],
			                                               FactoryBase.FactoryQueueName,
			                                               ht,
			                                               upgrade.guiTextureName,
			                                               (hud_ht) =>
		                                               		{			
																		DequeueUpgrade(hud_ht);
															});
			if (upgrade.unique)
			{
				upgrade.uniquelyUpgraded = true;
			}		
	}
	
	public virtual void EnqueueUnitToCreate (Unit unit)
	{
			gameplayManager.resources.UseResources (unit.costOfResources);
			invokeQueue.Add(queueTicket, unit);
						
			Hashtable ht = new Hashtable();
			ht["buttonType"] = unit;
			ht["name"] = "button-" + queueTicket;
			ht["queueSpot"] = queueTicket;		
		    queueTicket++;
			
			hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
			                                               FactoryBase.FactoryQueueName,
			                                               ht,
			                                               unit.guiTextureName,
			                                               (hud_ht) =>
			                                               {
																DequeueUnit(hud_ht);
															});
	}

	private void DequeueUnit(Hashtable ht)
	{
		string btnName 		 = (string)ht["name"];
		Unit unit			 = (Unit)ht["buttonType"];		
		int queueBtnSpot	 = (int)ht["queueSpot"];
		gameplayManager.resources.ReturnResources (unit.costOfResources);
		
		if(hudController.CheckQueuedButtonIsFirst(btnName, FactoryBase.FactoryQueueName))
		{
			inUpgrade = false;
			Debug.LogWarning("is first in queue");
			timer = 0;
			CancelInvokeSlider();
			unitToCreate = null;
		}
		
		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
		invokeQueue.Remove(queueBtnSpot);
		Debug.LogWarning("invokeQueue removido  = "+ queueBtnSpot);
		//		lUnitsToCreate.Remove (unit);
	}
	
	private void DequeueUpgrade (Hashtable ht)
	{
		string btnName  	 = (string)ht["name"];
		Upgrade upgrade		 = (Upgrade)ht["buttonType"];		
		int queueBtnSpot	 = (int)ht["queueSpot"];
		gameplayManager.resources.ReturnResources (upgrade.costOfResources);

		if (upgrade.unique)
		{
			upgrade.uniquelyUpgraded = false;
		}		

		foreach (UpgradeItem ui in upgradesToCreate)					// Procura o UpgradeItem referente ao dequeued Upgrade para reativar o botao
		{
			if(ui.upgrade == upgrade)
			{
				ui.alreadyUpgraded = false;
				techTreeController.UpgradeChildBoolOperator(ui.childTech.upgradeName, true);	
				break;
			}
		}
			
		if(hudController.CheckQueuedButtonIsFirst(btnName, FactoryBase.FactoryQueueName))
		{
			timer = 0;
			CancelInvokeSlider();
			upgradeToCreate = null;
			inUpgrade = false;
		}
		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
		invokeQueue.Remove(queueBtnSpot);

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

	private FactoryBase CheckAvailableFactory()
	{
		List<FactoryBase> factories = new List<FactoryBase> ();
		
		foreach (IStats stat in statsController.selectedStats)
		{
			FactoryBase factory = stat as FactoryBase;							
			if (factory == null) continue;							
			factories.Add (factory);
		}
		
		int i = 0, factoryChoose = 0, numberToCreate = -1;

		foreach (FactoryBase factory in factories)
		{
			if (numberToCreate == -1)
			{
				numberToCreate = factory.EnqueuedCount;
				factoryChoose = i;
			}
			else if (numberToCreate > factory.EnqueuedCount)
			{
				numberToCreate = factory.EnqueuedCount;
				factoryChoose = i;
			}
			i++;
		}
		
		if (!factories[factoryChoose].ReachedMaxEnqueued)
			return factories[factoryChoose];
		else
			eventController.AddEvent("reach enqueued units");
		return null;		
	}
	#endregion

	#region Invokes


	void InvokeUpgrade (Upgrade upgrade)
	{
		if (Selected)
		{
			hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);
			Invoke("RestoreDequeueMenu",0);
			Invoke("RestoreOptionsMenu",0);

		}
		Upgrade upg = Instantiate (upgrade, this.transform.position, Quaternion.identity) as Upgrade;
		upg.transform.parent = this.transform;
		if (upgrade.modelUpgrade) buildingState = BuildingState.Upgraded;
		eventController.AddEvent("standard message",transformParticleDamageReference.position , upgrade.name + " technology complete", upgrade.guiTextureName);	

	}
	
	public virtual void InvokeUnit (Unit unit)
	{	
		if (Selected)
		{
			hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);
			Invoke("RestoreDequeueMenu",0);
		}

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
			unitName = unit.name;
		}
		
		eventController.AddEvent("create unit",  transformParticleDamageReference.position, unitName, unit.guiTextureName);		
		PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();		
		string encodedBattle = (string)pw.GetPropertyOnRoom ("battle");
		
		if (!string.IsNullOrEmpty (encodedBattle))
		{
			Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));
			//Score
			Score.AddScorePoints (DataScoreEnum.UnitsCreated, 1);
			Score.AddScorePoints (DataScoreEnum.UnitsCreated, 1, battle.IdBattle);
			Score.AddScorePoints (unitName + DataScoreEnum.XCreated, 1);
			Score.AddScorePoints (unitName + DataScoreEnum.XCreated, 1, battle.IdBattle);
		}
		if (!hasRallypoint) return;
		
		// Look At
		Vector3 difference = goRallypoint.position - transform.position;
		Quaternion rotation = Quaternion.LookRotation (difference);
		Vector3 forward = rotation * Vector3.forward;		
		Vector3 unitSpawnPosition = transform.position + (forward * helperCollider.radius);

		Unit newUnit = null;
		if (PhotonNetwork.offlineMode)
		{
			Unit u = Instantiate (unit, unitSpawnPosition, Quaternion.identity) as Unit;
			newUnit = u;
		}
		else
		{
			GameObject u = PhotonNetwork.Instantiate(unit.gameObject.name, unitSpawnPosition, Quaternion.identity, 0);
			newUnit = u.GetComponent<Unit> ();
		}						
		newUnit.Init ();
				
		RallyPoint rallypoint = goRallypoint.GetComponent<RallyPoint> ();
		
		if (rallypoint.observedUnit != null)
		{
			newUnit.Follow (rallypoint.observedUnit);
		}

		if(newUnit.category == "Worker" && rallypoint.observedResource != null)
		{
			Worker workerUnit = (Worker)newUnit;
			workerUnit.SetResource (rallypoint.observedResource);
					
		}
		newUnit.Move (goRallypoint.position);
		newUnit.transform.parent = GameObject.Find("GamePlay/" + gameplayManager.MyTeam).transform;
	}

	private void InvokeSliderUpdate()
	{
		if(!isInvokingSlider) isInvokingSlider = true;
		buildingSlider.value = (timer / timeToCreate);
	}
	
	private void CancelInvokeSlider()
	{
		CancelInvoke ("InvokeSliderUpdate");
		isInvokingSlider = false;
		buildingSlider.value = 0;
	}
	#endregion
			
	#region IDeathObservable implementation
	
	public void RegisterDeathObserver (IDeathObserver observer)
	{
		IDOobservers.Add (observer);
	}
	
	public void UnRegisterDeathObserver (IDeathObserver observer)
	{
		IDOobservers.Remove (observer);
	}
	
	public void NotifyDeath ()
	{
		foreach (IDeathObserver o in IDOobservers)
		{
			o.OnObservableDie (this.gameObject);
		}
	}	

	#endregion

	#region TechBool e Attribs

	public void TechActiveBool(List<string> techs, bool isAvailable)
	{
		foreach (string tech in techs)
		{
			techTreeController.TechBoolOperator(tech,isAvailable);
		}
	}
	
	public void TechBool(string category, bool isAvailable)  // Aplica disponibilidade de tech em Upgrades e Units desta factory
	{
		foreach (UnitFactory uf in unitsToCreate)
		{
			if (uf.unit.category == category)
			{
				uf.techAvailable = isAvailable;
				break;
				
			}
		}
		foreach (UpgradeItem up in upgradesToCreate)
		{	
			if(up.upgrade.upgradeName == category)
			{
				up.techAvailable = isAvailable;
				break;
			}
		}
	}

	public void TechChildBool(string category, bool isTechChild)  // Aplica disponibilidade de tech em Upgrades e Units desta factory
	{
		foreach (UpgradeItem up in upgradesToCreate)
		{	
			if(up.upgrade.upgradeName == category)
			{
				up.isChildtech = isTechChild;
				break;
			}
		}
		if(Selected) RestoreOptionsMenu();
	}
	
	public void InitFactoryTechAvailability ()
	{
		foreach ( UnitFactory uc in unitsToCreate)
		{
			uc.techAvailable = uc.VIP;			
		}
		
		
		foreach (UpgradeItem up in upgradesToCreate)
		{	
			up.techAvailable = up.VIP;
		}		
	}

	public  void LoadStandardAttribs()											// Inicializa os tributos da unidade conforme Techtree
	{
		Hashtable ht = techTreeController.attribsHash[category] as Hashtable;
		bonusDefense 	= (int)ht["bonusdefense"];
		bonusProjectile = (int)ht["bonusprojectile"];
		bonusSight		= (int)ht["bonussight"];
	}

	#endregion

	#region RPC's
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

	[RPC]
	public void InstanceOverdraw (int teamID, int allyID)
	{
		wasBuilt = false;
		foreach (GameObject obj in buildingObjects.desactiveObjectsWhenInstance)
		{
			obj.SetActive (false);
		}
		
		SetTeam (teamID, allyID);
		
		levelConstruct = Health = 1;
		
		
		statsController.RemoveStats (GetComponent<FactoryBase> ());
		
		GetComponent<NavMeshObstacle> ().enabled = false;
		
		if (!photonView.isMine) model.SetActive (false);
		if (!PhotonNetwork.offlineMode) IsNetworkInstantiate = true;
		
		
	}
	
	[RPC]
	public void Instance ()
	{
		realRangeView  = this.fieldOfView;		
		GetComponent<NavMeshObstacle> ().enabled = true;		
		statsController.AddStats(this);		
		this.fieldOfView = 0.5f;
		
		foreach (GameObject obj in buildingObjects.desactiveObjectsWhenInstance)
		{
			obj.SetActive (true);
		}
		
		buildingState = BuildingState.Base;		
		SendMessage ("OnInstanceFactory", SendMessageOptions.DontRequireReceiver);
		
		if (!gameplayManager.IsSameTeam (team)) model.SetActive (true);

		hudController.CreateSubstanceConstructBar (this, sizeOfHealthBar, MaxHealth, true);
	}

	#endregion
}