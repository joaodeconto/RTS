using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Extension;

public class FactoryBase : IStats, IDeathObservable
{
	public const int MAX_NUMBER_OF_LISTED = 5;
	public const string FactoryQueueName = "Factory";
	
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
	
	public enum BuildingState
	{
		Base       = 0,
		Unfinished = 1,
		Finished   = 2,
		Upgraded   = 3
	}
	
	public UnitFactory[] unitsToCreate;
	public UpgradeItem[] upgradesToCreate;
	public Resource.Type receiveResource;
	public BuildingObjects buildingObjects;
	public string buttonName;
	public string guiTextureName;	
	public CapsuleCollider helperCollider { get; set; }	
	public bool hasRallypoint { get; set; }	
	public Transform goRallypoint;	
	public BuildingState buildingState { get; set; }
	protected int levelConstruct;	
	protected List<Unit> lUnitsToCreate = new List<Unit>();
	protected Unit unitToCreate;
	protected List<Upgrade> lUpgradesToCreate = new List<Upgrade>();
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
	
	[HideInInspector]
	public bool wasVisible = false;
	[HideInInspector]
	public bool alreadyCheckedMaxPopulation = false;	

	protected float realRangeView;	
	public bool IsDamaged {
		get	{
			return Health != MaxHealth;
		}
	}
	
	public bool ReachedMaxEnqueuedUnits	{
		get	{
			return lUnitsToCreate.Count + lUpgradesToCreate.Count  >= MAX_NUMBER_OF_LISTED;
		}
	}
	
	List<IDeathObserver> IDOobservers = new List<IDeathObserver> ();
	
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


	
	void Update ()
	{
		SyncAnimation ();
				
		if (!wasBuilt)
			return;

		if (lUnitsToCreate.Count == 0 && lUpgradesToCreate.Count == 0)	return;

		if (lUpgradesToCreate.Count >= 1)
		{
				if (upgradeToCreate == null)
				{
					upgradeToCreate = lUpgradesToCreate[0];
					timeToCreate = lUpgradesToCreate[0].timeToSpawn;
					inUpgrade = true;
					if (Selected)
					{
						buildingSlider.gameObject.SetActive(true);
						InvokeRepeating ("InvokeSliderUpdate",0.2f,0.2f);
					}
					hudController.CreateSubstanceResourceBar (this, sizeOfResourceBar, timeToCreate);					
				}
		}

		if (lUnitsToCreate.Count >= 1)
		{				
			if (gameplayManager.NeedMoreHouses (lUnitsToCreate[0].numberOfUnits))
			{
				if (!alreadyCheckedMaxPopulation)
				{
					alreadyCheckedMaxPopulation = true;
					eventController.AddEvent("need more houses",transformParticleDamageReference.position);
				}
				return;
			}

			else
				alreadyCheckedMaxPopulation = false;				
					
			if (unitToCreate == null)
			{
				unitToCreate = lUnitsToCreate[0];
				timeToCreate = lUnitsToCreate[0].timeToSpawn;
				inUpgrade = true;				
				if (Selected)
				{
					buildingSlider.gameObject.SetActive(true);
					InvokeRepeating ("InvokeSliderUpdate",0.2f,0.2f);                              			 // atualiza o slider a 0.2;
				}
				hudController.CreateSubstanceResourceBar (this, sizeOfResourceBar, timeToCreate);				
			}

		}
		
		if (inUpgrade)
		
		{
			if(!Selected && isInvokingSlider)
			{
				CancelInvoke ("InvokeSliderUpdate");
				buildingSlider.value = 0;
				isInvokingSlider = false;
			}
			if (timer > timeToCreate)
			{
				CancelInvoke ("InvokeSliderUpdate");
				isInvokingSlider = false;

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
				inUpgrade = false;
				timer = 0;			
			}
			else
			{
				timer += Time.deltaTime;	
				if(Selected && !isInvokingSlider) InvokeRepeating ("InvokeSliderUpdate",0.2f,0.2f);
			}
		}
	}

	#region Only About the Structure

	public override void SetVisible(bool isVisible)
	{
		
		statsController.ChangeVisibility (this, isVisible);
		
		if(isVisible)
		{
			model.transform.parent = this.transform;
			model.SetActive(true);
			
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
			
		yield return new WaitForSeconds (3f);
		if (IsNetworkInstantiate)
		{
			PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
			Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));
			
			if (photonView.isMine)
			{
				PhotonNetwork.Destroy(gameObject);
				
				//				Score.AddScorePoints (DataScoreEnum.BuildingsLost, 1);
				Score.AddScorePoints (DataScoreEnum.BuildingsLost, 1, battle.IdBattle);
				//				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1, battle.IdBattle);
			}
			else
			{
				//				Score.AddScorePoints (DataScoreEnum.DestroyedBuildings, 1);
				Score.AddScorePoints (DataScoreEnum.DestroyedBuildings, 1, battle.IdBattle);
				//				Score.AddScorePoints (this.category + DataScoreEnum.XDestroyed, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XDestroyed, 1, battle.IdBattle);
			}
		}
		else Destroy (gameObject);
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
				
				//				Score.AddScorePoints (DataScoreEnum.BuildingsCreated, 1);
				Score.AddScorePoints (DataScoreEnum.BuildingsCreated, 1, battle.IdBattle);
				//				Score.AddScorePoints (factoryName + DataScoreEnum.XCreated, 1);
				//				Score.AddScorePoints (factoryName + DataScoreEnum.XCreated, 1, battle.IdBattle);
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
	
	public override void Select ()
	{
		base.Select ();		

		hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));
		
		if (!playerUnit) return;
		
		if (wasBuilt)
		{
			foreach (UpgradeItem ui in upgradesToCreate)
			{
				if(!ui.techAvailable)
				{
					Hashtable ht = new Hashtable();
					ht["upgradeHT"] = ui;
					ht["disable"] = 0;
					
					hudController.CreateButtonInInspector (ui.buttonName,
					                                       ui.gridItemAttributes.Position,
					                                       ht,
					                                       ui.upgrade.guiTextureName,
					                                       null,
					                                       (ht_dcb, onClick) => 
					                                       {
																UpgradeItem upgrade = (UpgradeItem)ht_dcb["upgradeHT"];																
																hudController.OpenInfoBoxUpgrade(upgrade.upgrade, false);
															});
				}
				
				else
				{
					Hashtable ht = new Hashtable();
					ht["upgradeHT"] = ui;
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
																UpgradeItem upgrade = (UpgradeItem)ht_dcb["upgradeHT"];
															
																if (isDown)
																{
																	ht["time"] = Time.time;
																}
																else
																{
																	if (Time.time - (float)ht["time"] > 0.3f)
																	{	
																		hudController.OpenInfoBoxUpgrade(upgrade.upgrade, true);
																		
																	}
																	else
																	{
																		hudController.CloseInfoBox();
					                                       
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
																				numberToCreate = factory.lUpgradesToCreate.Count;
																				factoryChoose = i;
																																	}
																			else if (numberToCreate > factory.lUpgradesToCreate.Count)
																			{
																				numberToCreate = factory.lUpgradesToCreate.Count;
																				factoryChoose = i;
																			}
																			i++;
																		}
																		
																		if (!factories[factoryChoose].ReachedMaxEnqueuedUnits)
																		{
																				bool canBuy = gameplayManager.resources.CanBuy (upgrade.upgrade.costOfResources);
																																										
																				if (canBuy)
																				{
																					factories[factoryChoose].EnqueueUpgradeToCreate (upgrade.upgrade);

																						hudController.RemoveButtonInInspector (upgrade.buttonName);
																						upgrade.alreadyUpgraded = true;																																											
																				}					
																		}
																		
																		else
																			eventController.AddEvent("reach enqueued units");
																}
															}
							});
						}
				}
			}
			
				for(int i = 0; i != lUpgradesToCreate.Count; ++i)
				{
					Upgrade upgrade = lUpgradesToCreate[i];													
					Hashtable ht = new Hashtable ();
					ht["upgradeHT"] = upgrade;
					ht["name"] = "button-" + Time.time;					
					hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
					                                               FactoryBase.FactoryQueueName,
					                                               ht,
					                                               lUpgradesToCreate[i].guiTextureName,
					                                               (hud_ht) =>
					                                               {
	//					DequeueUpgrade(hud_ht);
				});
			}

			if (!hasRallypoint) return;			
			buildingSlider.gameObject.SetActive(true);			
			Transform boostBtn =  hudController.boostProduction.transform;
			Transform boostCostTr = boostBtn.transform.FindChild("Gold").FindChild("Label Gold Price");
			UILabel boostCostLabel = boostCostTr.GetComponent<UILabel>();
			boostCostLabel.text = boostCost.Rocks.ToString();
			
			DefaultCallbackButton dcb;
			
			dcb = ComponentGetter.Get<DefaultCallbackButton> (boostBtn, false);
			dcb.Init(null,
			         (ht_dcb) => 
			         {				
							canBoost = gameplayManager.resources.CanBuy (boostCost);
							
							if (onBoost)
							{
								eventController.AddEvent("standard message", "Already on Boosting Production ");
								return;
								
							}
							
							if (canBoost && !onBoost)
							{
								BoostProduction();
								gameplayManager.resources.UseResources (boostCost);
								Debug.Log("gold spent event");
								return;
								
							}
							
							else
							{
								eventController.AddEvent("out of funds", "boostCost");
								
							}
						});
	
			if (!goRallypoint.gameObject.activeSelf) goRallypoint.gameObject.SetActive (true);
						
			foreach (UnitFactory uf in unitsToCreate)
			{
				if(!uf.techAvailable)
				{
					Hashtable ht = new Hashtable();
					ht["unitFactory"] = uf;
					ht["disable"] = 0;

					hudController.CreateButtonInInspector (uf.buttonName,
					                                       uf.gridItemAttributes.Position,
					                                       ht,
					                                       uf.unit.guiTextureName,
					                                       null,
					                                       (ht_dcb, onClick) => 
					                                       {
																UnitFactory unitFactory = (UnitFactory)ht_dcb["unitFactory"];
															
																hudController.OpenInfoBoxUnit(unitFactory.unit,true);
															});							
				
				}
					
				else
				{
					Hashtable ht = new Hashtable();
					ht["unitFactory"] = uf;
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
																UnitFactory unitFactory = (UnitFactory)ht_dcb["unitFactory"];
															
																if (isDown)
																{
																	ht["time"] = Time.time;
																}
																else
																{
																	if (Time.time - (float)ht["time"] > 0.3f)
																	{	
																		hudController.OpenInfoBoxUnit(unitFactory.unit, false);																		
																	}
																	else
																	{
																		hudController.CloseInfoBox();																		
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
																				numberToCreate = factory.lUnitsToCreate.Count;
																				factoryChoose = i;
																			}
																			else if (numberToCreate > factory.lUnitsToCreate.Count)
																			{
																				numberToCreate = factory.lUnitsToCreate.Count;
																				factoryChoose = i;
																			}
																			i++;
																		}
																		
																		if (!factories[factoryChoose].ReachedMaxEnqueuedUnits)
																			factories[factoryChoose].EnqueueUnitToCreate (unitFactory.unit);
																		else
																			eventController.AddEvent("reach enqueued units");
																		
																	}
																}
															});
			}
		}
						
			for(int i = 0; i != lUnitsToCreate.Count; ++i)
			{
				Unit unit = lUnitsToCreate[i];
				
				Hashtable ht = new Hashtable ();
				ht["unit"] = unit;
				ht["name"] = "button-" + Time.time;
				
				hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
				                                               FactoryBase.FactoryQueueName,
				                                               ht,
				                                               lUnitsToCreate[i].guiTextureName,
				                                               (hud_ht) =>
				                                               {
					DequeueUnit(hud_ht);
				});
			}
			
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
			
			//			hudController.DestroyInspector ("factory");
		}
	}
	
	public void EnqueueUpgradeToCreate (Upgrade upgrade)
	{
		gameplayManager.resources.UseResources (upgrade.costOfResources);
		lUpgradesToCreate.Add (upgrade);
		Hashtable ht = new Hashtable();
		ht["upgradeHT"] = upgrade;
		ht["name"] = "button-" + Time.time;
		
		hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
		                                               FactoryBase.FactoryQueueName,
		                                               ht,
		                                               upgrade.guiTextureName,
		                                               (hud_ht) =>
		                                               {			
			//															DequeueUpgrade(hud_ht);
		});
		if (upgrade.unique)
		{
			upgrade.uniquelyUpgraded = true;
		}		
	}
	
	public virtual void EnqueueUnitToCreate (Unit unit)
	{
		bool canBuy = gameplayManager.resources.CanBuy (unit.costOfResources);
		
		if (canBuy)
		{
			gameplayManager.resources.UseResources (unit.costOfResources);
			lUnitsToCreate.Add (unit);
			Hashtable ht = new Hashtable();
			ht["unit"] = unit;
			ht["name"] = "button-" + Time.time;
			
			hudController.CreateEnqueuedButtonInInspector ( (string)ht["name"],
			                                               FactoryBase.FactoryQueueName,
			                                               ht,
			                                               unit.guiTextureName,
			                                               (hud_ht) =>
			                                               {
																DequeueUnit(hud_ht);
															});

		}
		else
			eventController.AddEvent("out of funds", unit.name);
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
		}
		
		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
		lUnitsToCreate.Remove (unit);
	}
	
//	private void DequeueUpgrade (Hashtable ht)
//	{
//		string btnName = (string)ht["name"];
//		Upgrade upgrade = 	 (Upgrade)ht["upgradeHT"];
//		
//		gameplayManager.resources.ReturnResources (upgrade.costOfResources);
//		if (upgrade.modelUpgrade || upgrade.unique)
//		{
//			Select();
//		}
//		
//		if(hudController.CheckQueuedButtonIsFirst(btnName, FactoryBase.FactoryQueueName))
//		{
//			timer = 0;
//			upgradeToCreate = null;
//		}
//		
//		hudController.RemoveEnqueuedButtonInInspector (btnName, FactoryBase.FactoryQueueName);
//		lUpgradesToCreate.Remove (upgrade);
//	}
	
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

	void InvokeUpgrade (Upgrade upgrade)
	{
		timer = 0;
		lUpgradesToCreate.RemoveAt (0);
		hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);	

		Upgrade upg = Instantiate (upgrade, this.transform.position, Quaternion.identity) as Upgrade;
		upg.transform.parent = this.transform;

		if (upgrade.modelUpgrade) buildingState = BuildingState.Upgraded;

		eventController.AddEvent("standard message",transformParticleDamageReference.position , "upgrade", upgrade.guiTextureName);	
	}
	
	public virtual void InvokeUnit (Unit unit)
	{				
		timer = 0;		
		lUnitsToCreate.RemoveAt (0);		
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
		
	public void BoostProduction ()
	{
		onBoost = true;
	}
			
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
	#endregion

	public  void LoadStandardAttribs()											// Inicializa os tributos da unidade conforme Techtree
	{
		Hashtable ht = techTreeController.attribsHash[category] as Hashtable;
		bonusDefense = (int)ht["bonusdefense"];
	}

	private void InvokeSliderUpdate()
	{
		if(!isInvokingSlider) isInvokingSlider = true;
		buildingSlider.value = (timer / timeToCreate);
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

