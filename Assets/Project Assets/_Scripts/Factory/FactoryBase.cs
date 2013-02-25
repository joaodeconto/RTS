using UnityEngine;
using System.Collections;

public class FactoryBase : MonoBehaviour {
	
	public Unit[] unitsToCreate;
	
	public int MaxHealth = 200;
	
	public int Health {get; private set;}
	public Animation ControllerAnimation {get; private set;}
	
	public int Team {get; protected set;}
	
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
	}

	
	void Awake ()
	{
//		Init ();
		
		enabled = false;
		Invoke ("Init", 0.1f);
	}
	
	void OnGUI ()
	{
	}
	
	public void Active ()
	{
		HealthBar healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);
		
		hudController.CreateSelected (transform, GetComponent<NavMeshObstacle>().radius, gameplayManager.GetColorTeam (Team));
	}
	
	public void Deactive ()
	{
		hudController.DestroySelected (transform);
	}
}
