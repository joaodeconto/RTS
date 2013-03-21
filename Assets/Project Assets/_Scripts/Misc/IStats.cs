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
				for (int k = 0; k != renderers[i].materials.Length; k++)
				{
					if (renderers[i].materials[k].name.Equals (materialToApplyColor.name + " (Instance)"))
					{
						renderers[i].materials[k].color = teamColor;
					}
				}
			}

			SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i != skinnedMeshRenderers.Length; i++)
			{
				for (int k = 0; k != skinnedMeshRenderers[i].materials.Length; k++)
				{
					if (skinnedMeshRenderers[i].materials[k].name.Equals (materialToApplyColor.name + " (Instance)"))
					{
						skinnedMeshRenderers[i].materials[k].color = teamColor;
					}
				}
			}
		}
	}

	[System.Serializable]
	public class ButtonAttributes
	{
		public string name;
		public string spriteName;
		public Vector2 position;
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

	public virtual void ReceiveAttack (int Damage)
	{
		if (Health == -1) return;

		int newDamage = Mathf.Max (0, Damage - Defense);

		Health = Mathf.Max (0, Health - newDamage);

		if (Health == 0)
		{
			gameplayManager.RemoveStatTeamID (Team);
			IsRemoved = true;

			SendMessage ("OnDie", SendMessageOptions.DontRequireReceiver);
			Health = -1;
		}
	}

	public GameObject model;
	public abstract void SetVisible(bool visible);
	public abstract bool IsVisible { get; }

	public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        IsNetworkInstantiate = true;
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
