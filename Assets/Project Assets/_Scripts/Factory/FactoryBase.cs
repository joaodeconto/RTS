using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactoryBase : MonoBehaviour {
	
	[System.Serializable]
	public class UnitFactory
	{
		public Unit unit;
		public float timeToCreate = 3f;
		public GameObject button;
		public Vector3 positionButton;
	}
	
	public UnitFactory[] unitsToCreate;
	protected Dictionary<float, Unit> listedToCreate = new Dictionary<float, Unit>(5);
	
	public int MaxHealth = 200;
	
	public int Health {get; private set;}
	public Animation ControllerAnimation {get; private set;}
	
	public int Team {get; protected set;}
	
	public bool Actived {get; protected set;}
	
	private float timer;
	
	protected GameplayManager gameplayManager;
	
	protected HUDController hudController;
	protected HealthBar healthBar;
	
	void Init ()
	{
		Health = MaxHealth;
		
		gameplayManager = GameController.GetInstance ().GetGameplayManager ();
		
		hudController = GameController.GetInstance ().GetHUDController ();
		
		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();
		
		GameController.GetInstance ().GetFactoryController ().AddFactory (this);
		
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
		
		int i = 0;
		foreach (KeyValuePair<float, Unit> listed in listedToCreate)
		{
			if (i == 0)
			{
				if (timer > listed.Key)
				{
					InvokeUnit (listed.Value);
					timer = 0;
					listedToCreate.Remove (listed.Key);
					break;
				}
				else
				{
					timer += Time.deltaTime;
				}
				i++;
			}
		}
	}
	
	void InvokeUnit (Unit unit)
	{
		Instantiate (unit, transform.position + (Vector3.forward * GetComponent<NavMeshObstacle>().radius), Quaternion.identity);
	}
	
	public void Active ()
	{
		if (!Actived) Actived = true;
		
		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);
		
		hudController.CreateSelected (transform, GetComponent<NavMeshObstacle>().radius, gameplayManager.GetColorTeam (Team));
		
		foreach (UnitFactory uf in unitsToCreate)
		{
			hudController.CreateButtonInInspector (uf.button, uf.positionButton, uf.unit, uf.timeToCreate, this);
		}
	}
	
	public void Deactive ()
	{
		if (Actived) Actived = false;
		
		hudController.DestroySelected (transform);
		
		hudController.DestroyInspector ();
	}
	
	public void CallUnit (Unit unit, float timeToCreate)
	{
		listedToCreate.Add (timeToCreate, unit);
	}
	
	public bool OverLimitCreateUnit ()
	{
		return listedToCreate.Count > 5;
	}
}