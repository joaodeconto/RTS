using UnityEngine;
using System.Collections;
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
		public int gridXIndex;
		public int gridYIndex;
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

	public int Team;
	public float RangeView;
	public float sizeOfSelected = 1f;

	public RendererTeamColor[] rendererTeamColor;

	public MovementAction[] movementActions;

	public bool playerUnit;

	public bool Selected { get; protected set; }
	public bool IsNetworkInstantiate { get; protected set; }
	public bool IsRemoved { get; protected set; }

	internal int Group = -1;

	protected GameplayManager gameplayManager;

	void Awake ()
	{
		Init();
	}

	public virtual void Init ()
	{
		//Change name
		this.name = this.name + (UniversalEntityCounter++);

		Health = MaxHealth;

		gameplayManager = ComponentGetter.Get<GameplayManager> ();

		if (IsNetworkInstantiate)
		{
			SetTeamInNetwork ();
		}
		else
		{
			if (Team < 0)
			{
				if (!PhotonNetwork.offlineMode)
				{
					SetTeamInNetwork ();
				}
				else
				{
					Team = (playerUnit) ? 0 : 1;
				}
			}
			else
			{
				playerUnit = gameplayManager.IsSameTeam (Team);
			}
		}

		SetColorTeam ();

		gameplayManager.AddStatTeamID (Team);

		IsRemoved = false;
	}

	public void ReceiveAttack (int Damage)
	{
		Debug.Log (Health);
		
		if (Health != 0)
		{
			int newDamage = Mathf.Max (0, Damage - Defense);
	
			Health = Mathf.Max (0, Health - newDamage);
		}

		if (Health == 0)
		{
			gameplayManager.RemoveStatTeamID (Team);
			IsRemoved = true;

			SendMessage ("OnDie", SendMessageOptions.DontRequireReceiver);
		}
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
			Team = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			PhotonPlayer other = PhotonPlayer.Find (photonView.ownerId);
			Team = (int)other.customProperties["team"];
		}
	}

	void SetColorTeam ()
	{
		foreach (RendererTeamColor rtc in rendererTeamColor)
		{
			rtc.SetColorInMaterial (transform, Team);
		}
	}
}
