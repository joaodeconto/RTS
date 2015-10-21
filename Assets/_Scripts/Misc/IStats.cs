using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Visiorama;
using PathologicalGames;

public abstract class IStats : Photon.MonoBehaviour, IHealthObservable
{
	#region Serializable		
	[System.Serializable]
	public class RendererTeamSubstanceColor
	{
		public Transform subMesh;
		private static Dictionary<string, ProceduralMaterial[]> unitTeamMaterials = new Dictionary<string, ProceduralMaterial[]> ();
		public void SetColorInMaterial (Transform transformMesh, int teamID, bool playerUnit)
		{
			Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
			Color teamColor1 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 1);
			Color teamColor2 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 2);
			string unitName = transformMesh.name;
			string keyUnitTeamMaterial = unitName + teamID;

			if (!unitTeamMaterials.ContainsKey (keyUnitTeamMaterial)){
				int nMaterials = subMesh.renderer.materials.Length;
				unitTeamMaterials.Add (keyUnitTeamMaterial, new ProceduralMaterial[nMaterials]);

				for (int i = 0, iMax = subMesh.renderer.materials.Length; i != iMax; ++i)
				{
					ProceduralMaterial substance 				  = subMesh.renderer.materials[i] as ProceduralMaterial;
					ProceduralPropertyDescription[] curProperties = substance.GetProceduralPropertyDescriptions();
					foreach (ProceduralPropertyDescription curProperty in curProperties)
					{
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
							substance.SetProceduralColor(curProperty.name, teamColor);
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor_1"))
							substance.SetProceduralColor(curProperty.name, teamColor1);
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor_2"))
							substance.SetProceduralColor(curProperty.name, teamColor2);
					}
					if(!playerUnit) substance.RebuildTextures();
					else     		substance.RebuildTexturesImmediately ();
					
					unitTeamMaterials[keyUnitTeamMaterial][i] = substance;
				}
			}
			ProceduralMaterial[] pms = unitTeamMaterials[keyUnitTeamMaterial];
			subMesh.renderer.sharedMaterials = pms as Material[];
		}
	}
	[System.Serializable]
	public class GridItemAttributes
	{
		public int gridXIndex;
		public int gridYIndex;
		public Vector3 Position
		{
			get{
				return ComponentGetter.Get<HUDController>().GetGrid("actions").GetGridPosition(gridXIndex, gridYIndex);
			}
		}
	}
	[System.Serializable]
	public class ButtonAttributes
	{
		public string name;
		public string spriteName;
		public GridItemAttributes gridItemAttributes;
	}
	[System.Serializable]
	public class MovementAction
	{
		public enum ActionType
		{
			Move,
			Patrol,
			CancelMovement,
			Follow, //Rally Point
			Attack, //Even same team
		}

		public ActionType actionType;
		public ButtonAttributes buttonAttributes;
	}
	#endregion

	#region Declares
	public int maxHealth = 200;
	private int m_health;
	public int Health {
		get { return m_health; }
		protected set {
			m_health = value; 
			NotifyHealthChange ();
		}
	}
	public int m_Team;
	public int team {
		get { return m_Team; }
		private set { m_Team = value; }
	}
	public int m_Ally;
	public int defense;
	public int group = -1;	
	public int bonusForce;
	public int bonusSpeed;
	public int bonusSight;
	public int bonusDefense;
	public int bonusProjectile;
	public int ally { get { return m_Team; } private set { m_Team = value; } }	
	public int totalResourceCost {get {return costOfResources.Rocks + costOfResources.Mana;}}

	public float fieldOfView;
	public float sizeOfSelected = 1f;
	public float sizeOfHealthBar {get {return (sizeOfSelected);}}
	public float sizeOfResourceBar {get {return (sizeOfSelected*1.2f);}}

	public bool playerUnit;
	public bool firstDamage {get; set; }
	public bool wasVisible { get; set; }
	public bool Selected { get; protected set; }
	public bool IsNetworkInstantiate { get; set; }
	public bool WasRemoved { get; protected set; }

	public string category;
	public string requisites;	
	public string subCategory;
	public string description;

	public GameObject model;
	public GameObject pref_ParticleDamage;
	public ResourcesManager costOfResources;
	public MovementAction[] movementActions;
	public Transform transformParticleDamageReference;
	public RendererTeamSubstanceColor[] rendererTeamSubstanceColor;

	private List<IHealthObserver> healthObservers = new List<IHealthObserver> ();
	private bool wasInitialized = false;

	public abstract void SetVisible(bool visible);
	public abstract bool IsVisible { get; }
	
	protected StatsController statsController;
	protected MiniMapController minimapController;
	protected HUDController hudController;
	protected GameplayManager gameplayManager;
	protected EventController eventController;
	protected SelectionController selectionController;
	protected TechTreeController techTreeController;
	
	public const int InvalidAlliance = 10000;
	public static int UniversalEntityCounter = 0;
	#endregion

	#region Awake, Init
	void Awake ()
	{		
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		if(ConfigurationData.InGame) Invoke ("Init", 0.1f);
	}

	public virtual void Init ()
	{
		if (wasInitialized)	return;
		wasInitialized = true;		
		techTreeController	=  ComponentGetter.Get<TechTreeController> ();
		statsController 	= ComponentGetter.Get<StatsController> ();
		hudController   	= ComponentGetter.Get<HUDController> ();
		eventController    	= ComponentGetter.Get<EventController> ();
		minimapController 	= ComponentGetter.Get<MiniMapController>();		
		selectionController = ComponentGetter.Get<SelectionController>();
		Health = MaxHealth;
		GhostFactory gf = GetComponent<GhostFactory>();

		if (!gameplayManager.IsBotTeam (this)){
			if (IsNetworkInstantiate){
				SetTeamInNetwork ();
			}
			else if (gf == null){
				if (!PhotonNetwork.offlineMode){
					SetTeamInNetwork ();
				}
				else{
					playerUnit = true;
					team = 0;
				}
			}
			else{
				playerUnit = true;
				team = 0;
			}
		}
		else playerUnit = false;
		firstDamage = false;
		SetColorTeam ();
		if (PhotonNetwork.offlineMode && gf == null){
			int idName = GetInstanceID();
			name = name + idName.ToString();
		}
		WasRemoved = false;
		RangedStructure rs = GetComponent<RangedStructure>();
		if(gf != null || rs != null) return;
		else statsController.AddStats (this);
	}
	#endregion

	#region General Methods	
	public void SetTeam (int team, int ally)
	{
		this.team = team;
		this.ally = ally;	
	}
	
	public virtual void Select ()
	{
		if (WasRemoved) return;		
		if (!Selected) Selected = true;
			else return;
	}
	
	public virtual void Deselect ()
	{
		if (Selected){
			Selected = false;						
//			hudController.DestroyOptionsBtns ();                //no deselectd factory e worker direto
			hudController.DestroySelected (transform);
			if(firstDamage) Invoke("ShowHealth",0.2f);
		}
		else return;
	}

	public void ShowHealth()
	{
		firstDamage = true;
		if(IsVisible && !WasRemoved)hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
	}

	public void ReceiveAttack (int Damage)
	{
		if (Health > 0){
			if(!firstDamage) ShowHealth();
			int newDamage = Mathf.Max (0, Damage - (defense+bonusDefense));
			Health = Mathf.Max (0, Health - newDamage);
			if (pref_ParticleDamage != null){
				if(!PhotonNetwork.offlineMode)	photonView.RPC ("InstantiatParticleDamage", PhotonTargets.All);
				else InstantiatParticleDamage();
			}			
			if (gameplayManager.IsBeingAttacked (this)){
				eventController.AddEvent("being attacked", transform.position);						
				minimapController.InstantiatePositionBeingAttacked (transform);			
			}
		}
		if (Health == 0 && !WasRemoved){
			SendRemove ();
			if(!PhotonNetwork.offlineMode)	photonView.RPC ("SendRemove", PhotonTargets.Others);
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		IsNetworkInstantiate = true;
	}
	
	public void SetHealth (int health)
	{
		Health = health;
	}
	
	void SetTeamInNetwork ()
	{
		playerUnit = photonView.isMine;
		if (playerUnit){
			team = (int)PhotonNetwork.player.customProperties["team"];
			if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
				ally = (int)PhotonNetwork.player.customProperties["allies"];
		}
		else{
			PhotonPlayer other = PhotonPlayer.Find (photonView.ownerId);			
			team = (int)other.customProperties["team"];
			if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
				ally = (int)other.customProperties["allies"];
		}
	}
	
	void SetColorTeam ()
	{		
		foreach (RendererTeamSubstanceColor rtsc in rendererTeamSubstanceColor)
		{
			rtsc.SetColorInMaterial (rtsc.subMesh, team, playerUnit);
		}
	}
	#endregion

	#region RPC's

	[RPC]
	public virtual void InstantiatParticleDamage ()
	{
		Transform newParticleDamage;

		if (transformParticleDamageReference != null){
			newParticleDamage = PoolManager.Pools["Particles"].Spawn(pref_ParticleDamage, transformParticleDamageReference.position, transformParticleDamageReference.rotation);
		}
		else{
			newParticleDamage = PoolManager.Pools["Particles"].Spawn(pref_ParticleDamage,transform.position, Quaternion.Euler (transform.forward));
		}
	}
	
	[RPC]
	public virtual void SendRemove ()
	{
		Health = 0;
		WasRemoved = true;		
		SendMessage ("OnDie", SendMessageOptions.DontRequireReceiver);
	}

	#endregion

	#region Gizmos
	void OnDrawGizmosSelected ()
	{
		DrawGizmosSelected ();
	}

	public virtual void DrawGizmosSelected ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere (this.transform.position, fieldOfView);
	}
	#endregion
	
	#region IHealthObservable implementation
	
	public int MaxHealth {
		get {
			return maxHealth;
		}
	}	
	public void RegisterHealthObserver (IHealthObserver observer)
	{
		healthObservers.Add (observer);
	}	
	public void UnRegisterHealthObserver (IHealthObserver observer)
	{
		healthObservers.Remove (observer);
	}	
	public void NotifyHealthChange ()
	{	
		foreach (IHealthObserver o in healthObservers)
		{
			o.UpdateHealth (m_health);
		}
	}
	#endregion
}
