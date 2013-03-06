using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FactoryBase : IStats
{
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

	public Transform waypoint;

	public bool playerUnit;

	public Animation ControllerAnimation {get; private set;}

	protected GameplayManager gameplayManager;

	protected HUDController hudController;
	protected HealthBar healthBar;

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

		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();

		if (Team == -1)
		{
			if (!PhotonNetwork.offlineMode)
			{
				Team = (int)PhotonNetwork.player.customProperties["team"];
			}
			else
			{
				Team = 0;
			}
		}

		if (waypoint == null) waypoint = transform.FindChild("Waypoint");

		waypoint.gameObject.SetActive (false);

		playerUnit = gameplayManager.IsSameTeam (this);

		this.gameObject.tag = "Factory";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		if (!enabled) enabled = playerUnit;

		ComponentGetter.Get<FactoryController> ().AddFactory (this);
	}

	void Awake ()
	{
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
				unitToCreate = null;
			}
			else
			{
				timer += Time.deltaTime;
			}
		}
	}

	void InvokeUnit (Unit unit)
	{
//		Vector3 unitSpawnPosition = transform.position + (transform.forward * GetComponent<CapsuleCollider>().radius);

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
		ComponentGetter.Get<FactoryController> ().RemoveFactory (this);
//		if (PhotonNetwork.offlineMode) Destroy (gameObject);
//		else PhotonNetwork.Destroy(gameObject);
		Destroy (gameObject);
	}

	public void Active ()
	{
		if (!Actived) Actived = true;
		else return;

		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, GetComponent<CapsuleCollider>().radius, gameplayManager.GetColorTeam (Team));

		if (playerUnit)
		{
			waypoint.gameObject.SetActive (true);

			foreach (UnitFactory uf in unitsToCreate)
			{
				hudController.CreateButtonInInspector (uf.buttonName, uf.positionButton, uf.unit, this);
			}
		}
	}

	public bool Deactive ()
	{
		if (waypoint.GetComponent<CreationPoint> ().active) return false;

		if (Actived) Actived = false;
		else return false;

		hudController.DestroySelected (transform);

		if (playerUnit)
		{
			waypoint.gameObject.SetActive (false);

			hudController.DestroyInspector ();
		}

		return true;
	}

	public void CallUnit (Unit unit)
	{
		listedToCreate.Add (unit);
	}

	public override void SetVisible(bool visible)
	{
		model.SetActive(visible);
	}

	public override bool IsVisible
	{
		get
		{
			return model.activeSelf;
		}
	}
}
