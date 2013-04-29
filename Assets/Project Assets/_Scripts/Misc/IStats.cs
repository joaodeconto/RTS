using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Visiorama;

public abstract class IStats : Photon.MonoBehaviour
{
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

	public int Health { get; protected set; }
	public int MaxHealth = 200;
	public int Defense;

	public int team;
	public float fieldOfView;
	public float sizeOfSelected = 1f;

	public RendererTeamColor[] rendererTeamColor;

	public MovementAction[] movementActions;

	public GameObject pref_ParticleDamage;
	public Transform transformParticleDamageReference;

	public bool playerUnit;

	public bool Selected { get; protected set; }
	public bool IsNetworkInstantiate { get; protected set; }
	public bool IsRemoved { get; protected set; }

	internal int Group = -1;

	protected GameplayManager gameplayManager;
	protected EventManager eventManager;

	void Awake ()
	{
		Init();
	}

	public virtual void Init ()
	{
		Health = MaxHealth;

		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		eventManager    = ComponentGetter.Get<EventManager> ();

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
				team = (playerUnit) ? 0 : 1;
			}

		}

		SetColorTeam ();

		gameplayManager.AddStatTeamID (team);

		IsRemoved = false;
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
			}
		}

		if (Health == 0 && !IsRemoved)
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
		gameplayManager.RemoveStatTeamID (team);
		IsRemoved = true;
		
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
		}
		else
		{
			PhotonPlayer other = PhotonPlayer.Find (photonView.ownerId);
			team = (int)other.customProperties["team"];
		}
	}

	void SetColorTeam ()
	{
		foreach (RendererTeamColor rtc in rendererTeamColor)
		{
			rtc.SetColorInMaterial (transform, team);
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
}
