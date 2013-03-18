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
		public Vector3 positionButton;
	}

	public UnitFactory[] unitsToCreate;

	public Transform waypoint;

	public string guiTextureName;
	public string unitCreatedEventMessage;

	protected List<Unit> listedToCreate = new List<Unit>();
	protected Unit unitToCreate;
	protected float timeToCreate;
	protected float timer;
	protected bool inUpgrade;

	public Animation ControllerAnimation { get; private set; }

	public bool wasBuilt { get; private set; }

	protected FactoryController factoryController;
	protected HUDController hudController;
	protected EventManager eventManager;
	protected HealthBar healthBar;

	public bool wasVisible = false;

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
		eventManager      = ComponentGetter.Get<EventManager> ();
		factoryController = ComponentGetter.Get<FactoryController> ();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();

		if (waypoint == null) waypoint = transform.FindChild("Waypoint");

		waypoint.gameObject.SetActive (false);

		playerUnit = gameplayManager.IsSameTeam (this);

		this.gameObject.tag = "Factory";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		factoryController.AddFactory (this);

		inUpgrade = false;
		wasBuilt = true;

		enabled = playerUnit;
	}

	void Update ()
	{
		if (listedToCreate.Count == 0) return;

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

	void InvokeUnit (Unit unit)
	{
		listedToCreate.RemoveAt (0);

		hudController.DequeueButtonInInspector(FactoryBase.FactoryQueueName);

		eventManager.AddEvent(unitCreatedEventMessage + " " + unit.name, unit.guiTextureName);

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

	public void Instance (int teamID)
	{
		Health = 1;
		wasBuilt = false;
		enabled = false;
		Team = teamID;
	}

	public bool Construct (Worker worker)
	{
		if (Health == MaxHealth)
		{
			worker.SetMoveToFactory (this);
			wasBuilt = true;
			return false;
		}
		else
		{
			Health += worker.constructionAndRepairForce;
			Health = Mathf.Clamp (Health, 0, MaxHealth);
			return true;
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
			waypoint.gameObject.SetActive (true);
			if (!waypoint.gameObject.activeSelf)
				waypoint.gameObject.SetActive (true);

			foreach (UnitFactory uf in unitsToCreate)
			{
				Hashtable ht = new Hashtable();
				ht["unit"]    = uf.unit;

				hudController.CreateButtonInInspector ( uf.buttonName,
														uf.positionButton,
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

		if(hudController.CheckQueuedButtonIsFirst(FactoryBase.FactoryQueueName, btnName))
		{
			Debug.Log("chegouvids");
			timer = 0;
			unitToCreate = null;
			inUpgrade = false;
		}

		hudController.RemoveEnqueuedButtonInInspector (FactoryBase.FactoryQueueName, btnName);
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
