using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FactoryBase : MonoBehaviour {
	
	public const int MAX_NUMBER_OF_LISTED = 5;
	
	[System.Serializable]
	public class UnitFactory
	{
		public Unit unit;
		public float timeToCreate = 3f;
		public string buttonName;
		public Vector3 positionButton;
	}
	
	public UnitFactory[] unitsToCreate;
	protected List<Unit> listedToCreate = new List<Unit>();
	protected Unit unitToCreate;
	protected float timeToCreate;
	private float timer;

	public int MaxHealth = 200;

	public int Health {get; private set;}
	public Animation ControllerAnimation {get; private set;}

	public int Team {get; protected set;}

	public bool Actived {get; protected set;}

	protected GameplayManager gameplayManager;

	protected HUDController hudController;
	protected HealthBar healthBar;
	
	public bool OverLimitCreateUnit
	{
		get
		{
			return listedToCreate.Count > MAX_NUMBER_OF_LISTED;
		}
	}

	void Init ()
	{
		Health = MaxHealth;

		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();

		ComponentGetter.Get<FactoryController> ().AddFactory (this);

		if (!PhotonNetwork.offlineMode)
		{
			Team = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			Team = 0;
		}

		this.gameObject.tag = "Factory";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		if (!enabled) enabled = true;

		timer = 0;
	}

	void Awake ()
	{
//		Init ();

		enabled = false;
		Invoke ("Init", 0.1f);
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
		}
		else
		{
			if (timer > timeToCreate)
			{
				InvokeUnit (unitToCreate);
				timer = 0;
				listedToCreate.Remove (unitToCreate);
			}
			else
			{
				timer += Time.deltaTime;
			}
		}
	}

	void InvokeUnit (Unit unit)
	{
		if (PhotonNetwork.offlineMode)
		{
			Unit newUnit = 
				Instantiate (unit, transform.position + (Vector3.forward * GetComponent<NavMeshObstacle>().radius), Quaternion.identity) as Unit;
//			newUnit.Move (Vector3.zero);
		}
		else
		{
	        GameObject newUnit = 
				PhotonNetwork.Instantiate(unit.gameObject.name, 
					transform.position + (Vector3.forward * GetComponent<NavMeshObstacle>().radius), Quaternion.identity, 0);
//			newUnit.GetComponent<Unit> ().Move (Vector3.zero);
		}
	}

	public void Active ()
	{
		if (!Actived) Actived = true;

		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, GetComponent<NavMeshObstacle>().radius, gameplayManager.GetColorTeam (Team));

		foreach (UnitFactory uf in unitsToCreate)
		{
			hudController.CreateButtonInInspector (uf.buttonName, uf.positionButton, uf.unit, this);
		}
	}

	public void Deactive ()
	{
		if (Actived) Actived = false;

		hudController.DestroySelected (transform);

		hudController.DestroyInspector ();
	}

	public void CallUnit (Unit unit)
	{
		listedToCreate.Add (unit);
	}

}
