using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using Visiorama;

public abstract class IStats : Photon.MonoBehaviour, IHealthObservable
{
	public const int InvalidAlliance = 10000;

	public static int UniversalEntityCounter = 0;

	[System.Serializable]
	public class RendererTeamColor
	{
		public Material materialToApplyColor;

		public void SetColorInMaterial (Transform transform, int teamID)
		{
			Color teamColor = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID);

			MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i != renderers.Length; i++)
			{
				Material[] materials = renderers[i].materials;
				for (int k = 0; k != materials.Length; k++)
				{
					Material m = materials[k];
					if (m.name.Equals (materialToApplyColor.name + " (Instance)"))
					{
						m.color = teamColor;
					}
				}
			}

			SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i != skinnedMeshRenderers.Length; i++)
			{
				Material[] materials = skinnedMeshRenderers[i].materials;
				for (int k = 0; k != materials.Length; k++)
				{
					Material m = materials[k];
					if (m.name.Equals (materialToApplyColor.name + " (Instance)"))
					{
						m.color = teamColor;
					}
				}
			}
		}
	}
	
	[System.Serializable]
	public class RendererTeamSubstanceColor
	{
		public Transform subMesh;

		private static Dictionary<string, ProceduralMaterial[]> unitTeamMaterials = new Dictionary<string, ProceduralMaterial[]> ();

		//Caso esse metodo for modificado eh necessario modificar no Rallypoint tbm
		public void SetColorInMaterial (Transform transform, int teamID)
		{
			Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
			Color teamColor1 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 1);
			Color teamColor2 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 2);

			string unitName = transform.name;
			int startRemoveIndex = unitName.IndexOf ("(");
			
			unitName = Regex.Replace (unitName, "[0-9]", "" );
			
			startRemoveIndex = (startRemoveIndex > 0) ? startRemoveIndex : unitName.Length - 1;
			unitName.Remove (startRemoveIndex);

			string keyUnitTeamMaterial = unitName + " - " + teamID;
			
			//Inicializando unitTeamMaterials com materiais compartilhado entre as unidades iguais de cada time
			if (!unitTeamMaterials.ContainsKey (keyUnitTeamMaterial))
			{
				int nMaterials = subMesh.renderer.materials.Length;
				unitTeamMaterials.Add (keyUnitTeamMaterial, new ProceduralMaterial[nMaterials]);

				for (int i = 0, iMax = subMesh.renderer.materials.Length; i != iMax; ++i)
				{
					ProceduralMaterial substance 				  = subMesh.renderer.materials[i] as ProceduralMaterial;
					ProceduralPropertyDescription[] curProperties = substance.GetProceduralPropertyDescriptions();
					
					//Setando os valores corretos de cor
					foreach (ProceduralPropertyDescription curProperty in curProperties)
					{
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
							substance.SetProceduralColor(curProperty.name, teamColor);
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor_1"))
							substance.SetProceduralColor(curProperty.name, teamColor1);
						if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor_2"))
							substance.SetProceduralColor(curProperty.name, teamColor2);
					}

					substance.RebuildTextures ();

					unitTeamMaterials[keyUnitTeamMaterial][i] = substance;
				}
			}

			//Associando na unidade os materiais corretos
			ProceduralMaterial[] pms = unitTeamMaterials[keyUnitTeamMaterial];
//			List<Material> mms = new List<Material> ();
//
//			foreach (ProceduralMaterial pm in pms)
//			{
//				mms.Add (pm)
//			}
			subMesh.renderer.sharedMaterials = pms as Material[];

		}
	}

	/*
	 //Old code

		public Transform subMesh;

		private ProceduralMaterial substance;
		private ProceduralPropertyDescription[] curProperties;

		public void SetColorInMaterial (Transform transform, int teamID)
		{
			Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
			Color teamColor1 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 1);
			Color teamColor2 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 2);

			substance 	  = subMesh.renderer.sharedMaterial as ProceduralMaterial;
			curProperties = substance.GetProceduralPropertyDescriptions();
			
			foreach (ProceduralPropertyDescription curProperty in curProperties)
			{
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
					substance.SetProceduralColor(curProperty.name, teamColor);
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor1"))
					substance.SetProceduralColor(curProperty.name, teamColor1);
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor2"))
					substance.SetProceduralColor(curProperty.name, teamColor2);
			}
		}
	 */
	[System.Serializable]
	public class GridItemAttributes
	{
		public int gridXIndex;
		public int gridYIndex;
		public Vector3 Position
		{
			get
			{
				return ComponentGetter
							.Get<HUDController>()
								.GetGrid("actions")
									.GetGridPosition(gridXIndex, gridYIndex);

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

	public int maxHealth = 200;
	private int m_health;
	public int Health {
		get { return m_health; }
		protected set {
			m_health = value; 
			NotifyHealthChange ();
		}
	}
	
	public int Defense;

	public int m_Team;
	public int team {
		get { return m_Team; }
		private set { m_Team = value; }
	}
	
	public int m_Ally;
	public int ally { get { return m_Team; } private set { m_Team = value; } }
	
	public float fieldOfView;
	public float sizeOfSelected = 1f;
	public float sizeOfSelectedHealthBar = 1f;

//	public RendererTeamColor[] rendererTeamColor;
	public RendererTeamSubstanceColor[] rendererTeamSubstanceColor;

	public MovementAction[] movementActions;

	public GameObject pref_ParticleDamage;
	public Transform transformParticleDamageReference;

	public bool playerUnit;

	public string category;
	
	public ResourcesManager costOfResources;
	
	public bool Selected { get; protected set; }
	public bool IsNetworkInstantiate { get; protected set; }
	public bool WasRemoved { get; protected set; }
	
	internal int Group = -1;
	
	protected StatsController statsController;
	protected HUDController hudController;
	protected GameplayManager gameplayManager;
	protected EventManager eventManager;

	//IHealthObserver
	private List<IHealthObserver> healthObservers = new List<IHealthObserver> ();

	void Awake ()
	{
		Init ();
	}

	public virtual void Init ()
	{		
		statsController = ComponentGetter.Get<StatsController> ();
		hudController   = ComponentGetter.Get<HUDController> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		eventManager    = ComponentGetter.Get<EventManager> ();
		
		Health = MaxHealth;
		
		if (!gameplayManager.IsBotTeam (this))
		{
			if (IsNetworkInstantiate)
			{
				SetTeamInNetwork ();
			}
			else
			{
				if (!PhotonNetwork.offlineMode)
				{
					SetTeamInNetwork ();
				}
				else
				{
					team = GameplayManager.BOT_TEAM;
				}
			}
		}
		else
		{
			playerUnit = false;
		}

		SetColorTeam ();
		
		WasRemoved = false;
		
		statsController.AddStats (this);

// =================================================================
// |                                                               |
// |      UTILIZANDO COMPONENTE FOW EM TODAS UNIDADES!!!           |
// |                 DEVE SER ATUALIZADO!!!                        |
// |                                                               |
// =================================================================
//	
//		if (!gameplayManager.IsBoot (team))
//		{
//			if (gameplayManager.SameEntity (team, ally))
//			{
//				FOWRevealer fowr = gameObject.AddComponent<FOWRevealer>();
//				fowr.range = new Vector2(0, fieldOfView);
//				fowr.lineOfSightCheck = FOWSystem.LOSChecks.EveryUpdate;
//			}
//			else
//			{
//				gameObject.AddComponent<FOWRenderers>();
//			}
//		}
//		else
//		{
//			gameObject.AddComponent<FOWRenderers>();
//		}
	}
	
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
		if (Selected) Selected = false;
		else return;
	}

	public void ReceiveAttack (int Damage)
	{
		if (Health != 0)
		{
			int newDamage = Mathf.Max (0, Damage - Defense);

			Health = Mathf.Max (0, Health - newDamage);

			if (pref_ParticleDamage != null)
			{
				photonView.RPC ("InstantiatParticleDamage", PhotonTargets.All);
			}
			
			if (gameplayManager.IsBeingAttacked (this))
			{
				eventManager.AddEvent("being attacked");
						
				Visiorama.ComponentGetter.Get<MiniMapController> ().InstantiatePositionBeingAttacked (transform);

				AudioClip sfxbeingattacked = SoundManager.Load("being_attacked");
				
				SoundManager.PlaySFX(sfxbeingattacked);
			}
		}

		if (Health == 0 && !WasRemoved)
		{
			SendRemove ();
			photonView.RPC ("SendRemove", PhotonTargets.Others);
		}
	}

	[RPC]
	public virtual void InstantiatParticleDamage ()
	{
		GameObject newParticleDamage;

		if (transformParticleDamageReference != null)
		{
			newParticleDamage = Instantiate (pref_ParticleDamage, transformParticleDamageReference.position, transformParticleDamageReference.rotation) as GameObject;
		}
		else
		{
			newParticleDamage = Instantiate (pref_ParticleDamage, transform.position, Quaternion.Euler (transform.forward)) as GameObject;
		}
	}
	
	[RPC]
	public virtual void SendRemove ()
	{
		Health = 0;
		WasRemoved = true;
		
		SendMessage ("OnDie", SendMessageOptions.DontRequireReceiver);
	}

	public GameObject model;
	public abstract void SetVisible(bool visible);
	public abstract bool IsVisible { get; }

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
		if (playerUnit)
		{
			team = (int)PhotonNetwork.player.customProperties["team"];
			if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
				ally = (int)PhotonNetwork.player.customProperties["allies"];
		}
		else
		{
			PhotonPlayer other = PhotonPlayer.Find (photonView.ownerId);
			
			team = (int)other.customProperties["team"];
			if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
				ally = (int)other.customProperties["allies"];
		}
	}

	void SetColorTeam ()
	{
//		foreach (RendererTeamColor rtc in rendererTeamColor)
//		{
//			rtc.SetColorInMaterial (transform, team);
//		}

		foreach (RendererTeamSubstanceColor rtsc in rendererTeamSubstanceColor)
		{
			rtsc.SetColorInMaterial (transform, team);
		}
	}

	// GIZMOS
	void OnDrawGizmosSelected ()
	{
		DrawGizmosSelected ();
	}

	public virtual void DrawGizmosSelected ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere (this.transform.position, fieldOfView);
	}
	
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
		if (IsVisible && !hudController.HasSelected(this.transform))
		{
			hudController.CreateSubstanceHealthBar(this, sizeOfSelectedHealthBar, MaxHealth, "Health Reference");
			Invoke ("DestroySelected", 5.0f);

		}

//		if (!team

		foreach (IHealthObserver o in healthObservers)
		{
			o.UpdateHealth (m_health);
		}
	}


	#endregion
}
