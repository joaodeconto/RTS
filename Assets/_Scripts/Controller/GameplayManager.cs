using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;
using Visiorama.Utils;

[System.Serializable]
public class Team
{
	public string name;
	public Color[] colors = new Color[3] { Color.white, Color.gray, Color.blue };
	public Texture2D colorTexture;
	public Transform initialPosition;

	public bool lose { get; set; }
	
	public void CreateColorTexture ()
	{
		colorTexture = new Texture2D(1, 1);
		
		colorTexture.SetPixel (0, 0, Color.blue);
		
		colorTexture.Apply ();
	}
}

public class GameplayManager : Photon.MonoBehaviour
{
	[System.Serializable]
	public class HUD
	{    
		public UILabel labelRocks;
		public UILabel labelMana;
		public UILabel labelUnits;
		public GameObject uiVictoryObject;
		public GameObject uiDefeatObject;
		public UILabel labelTime;
		public GameObject uiLostMainBaseObject;
	}

	public class AllyClass
	{
		public int ally;
		public List<int> teams;

		public AllyClass (int ally)
		{
			this.ally = ally;
			teams = new List<int>();
		}
	}

	public enum Mode
	{
		Deathmatch,
		Cooperative
	}

	public static Mode mode;

	public const int MAX_POPULATION_ALLOWED = 200;
	public const int BOOT_TEAM = 8;

	public Team[] teams;

	public int numberOfUnits { get; protected set; }
	public int maxOfUnits { get; protected set; }
	protected int mainBasesIncrements;
	protected int excessHousesIncrements;
	protected int numberOfHousesMore;

	protected List<AllyClass> alliesNumberOfStats = new List<AllyClass>();
	protected int loserTeams;
	protected int numberOfTeams;
	protected bool loseGame = false;
	protected bool winGame = false;
	protected float currentTime;

	protected bool beingAttacked = false;

	public int MyTeam {get; protected set;}
	public int Allies {get; protected set;}

	// Resources
	public ResourcesManager resources;

	public HUD hud;

	protected NetworkManager network;

	public void Init ()
	{
		if (!PhotonNetwork.offlineMode)
		{
			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
			if (mode == Mode.Cooperative)
			{
				Allies = (int)PhotonNetwork.player.customProperties["allies"];
			}
			else
			{
				numberOfTeams = PhotonNetwork.room.maxPlayers;
			}

			network = ComponentGetter.Get<NetworkManager>();
		}
		else
		{
			MyTeam = 0;
			Allies = 0;
		}

		for (int i = 0; i != teams.Length; i++)
		{
			if (MyTeam == i)
			{
				Math.CenterCameraInObject (Camera.main, teams[i].initialPosition.position);
			}
			
			teams[i].CreateColorTexture ();
		}

		if (mode == Mode.Cooperative)
		{
			alliesNumberOfStats.Add (new AllyClass (0));
			alliesNumberOfStats.Add (new AllyClass (1));

			for (int i = alliesNumberOfStats.Count - 1; i != -1; --i)
			{
				foreach (PhotonPlayer pp in PhotonNetwork.playerList)
				{
					if (alliesNumberOfStats[i].ally == (int)pp.customProperties["allies"])
					{
						alliesNumberOfStats[i].teams.Add ((int)pp.customProperties["team"]);
					}
				}
			}
		}
		else
		{
			loserTeams = 0;
		}

		hud.uiDefeatObject.SetActive (false);
		hud.uiVictoryObject.SetActive (false);
		hud.uiLostMainBaseObject.SetActive (false);

		numberOfHousesMore = excessHousesIncrements = 0;
		
		InvokeRepeating ("EndMatch", 10f, 10f);
	}
	
	/// <summary>
	/// Gets the texture color of my team.
	/// </summary>
	/// <returns>
	/// The of my texture color team.
	/// </returns>
	/// 
	public Texture2D GetColorTextureTeam ()
	{
		return GetColorTextureTeam (MyTeam);
	}
	
	/// <summary>
	/// Gets the texture color team.
	/// </summary>
	/// <returns>
	/// The texture color team.
	/// </returns>
	/// <param name='teamID'>
	/// Team ID.
	/// </param>
	public Texture2D GetColorTextureTeam (int teamID)
	{
		if (teamID >= 0 && teamID < teams.Length)
		{
			return teams[teamID].colorTexture;
		}
		else
		{
			Debug.LogError ("Team ID not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
			return null;
		}
	}
	
	/// <summary>
	/// Gets the color of my team.
	/// </summary>
	/// <returns>
	/// The of my color team.
	/// </returns>
	/// 
	public Color GetColorTeam ()
	{
		return GetColorTeam (MyTeam, 0);
	}

	/// <summary>
	/// Gets the color team.
	/// </summary>
	/// <returns>
	/// The color team.
	/// </returns>
	/// <param name='teamID'>
	/// Team ID.
	/// </param>
	public Color GetColorTeam (int teamID) { return GetColorTeam (teamID, 0); }
	public Color GetColorTeam (int teamID, int indexColor)
	{
		if (teamID >= 0 && teamID < teams.Length)
		{
			if (indexColor >= 0 && indexColor < teams[teamID].colors.Length)
			{
				return teams[teamID].colors[indexColor];
			}
			else
			{
				Debug.LogError ("The Team does not have the indexColor " + indexColor);
				return Color.black;
			}
		}
		else
		{
			Debug.LogError ("Team ID does not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
			return Color.black;
		}
	}

	public bool IsSameTeam (int teamID)
	{
		return teamID == MyTeam;
	}

	public bool IsSameTeam (IStats stats)
	{
		return stats.team == MyTeam;
	}

	public bool IsBoot (int team)
	{
		return team == BOOT_TEAM;
	}

	public bool IsAlly (int allyNumber)
	{
		return allyNumber == Allies;
	}

	public bool IsAlly (IStats stats)
	{
		return stats.ally == Allies;
	}

	public bool SameEntity (int teamID, int ally)
	{
		if (GameplayManager.mode == Mode.Cooperative)
		{
			return IsAlly (ally);
		}
		else
		{
			return IsSameTeam (teamID);
		}
	}

	public bool IsBeingAttacked (IStats target)
	{
		if (!beingAttacked)
		{
			if (IsSameTeam (target))
			{
				bool isInCamera = ComponentGetter.Get<TouchController> ().IsInCamera (target.transform.position);

				if (!isInCamera)
				{
					beingAttacked = true;
					Invoke ("BeingAttackedToFalse", 5f);
					GetComponent<SoundSource> ().Play ("BeingAttacked");

					return true;
				}
			}
		}

		return false;
	}

	void BeingAttackedToFalse ()
	{
		beingAttacked = false;
	}

	public void IncrementMainBase (int teamID)
	{
		if (IsSameTeam (teamID))
		{
			mainBasesIncrements++;
			hud.uiLostMainBaseObject.SetActive (false);
			CancelInvoke ("NoMainBase");
			CancelInvoke ("DecrementTime");
		}
	}

	public void DecrementMainBase (int teamID)
	{
		if (IsSameTeam (teamID))
		{
			mainBasesIncrements--;
			if (mainBasesIncrements == 0)
			{
				hud.uiLostMainBaseObject.SetActive (true);
				currentTime = 40f;
				hud.labelTime.text = currentTime.ToString () + "s";
				Invoke ("NoMainBase", currentTime);
				InvokeRepeating ("DecrementTime", 1f, 1f);
			}
		}
	}

	public void IncrementUnit (int teamID, int numberOfUnits)
	{
		if (IsSameTeam (teamID)) this.numberOfUnits += numberOfUnits;
	}

	public void DecrementUnit (int teamID, int numberOfUnits)
	{
		if (IsSameTeam (teamID)) this.numberOfUnits -= numberOfUnits;
	}

	public void IncrementMaxOfUnits (int numberOfIncrementUnits)
	{
		if (!ReachedMaxPopulation)
		{
			maxOfUnits += numberOfIncrementUnits;
			if (maxOfUnits >= MAX_POPULATION_ALLOWED)
			{
				numberOfHousesMore = maxOfUnits - MAX_POPULATION_ALLOWED;
				maxOfUnits -= numberOfHousesMore;
			}
		}
		else ++excessHousesIncrements;
	}

	public void DecrementMaxOfUnits (int numberOfDecrementUnits)
	{
		if (excessHousesIncrements == 0)
		{
			maxOfUnits -= numberOfDecrementUnits;
			if (numberOfHousesMore != 0) maxOfUnits += numberOfHousesMore;
		}
		else --excessHousesIncrements;
	}

	public bool NeedMoreHouses (int additionalUnits)
	{
		return (numberOfUnits + additionalUnits > maxOfUnits);
	}

	public bool ReachedMaxPopulation
	{
		get
		{
			return (maxOfUnits >= MAX_POPULATION_ALLOWED);
		}
	}

	public void CheckCondition (int teamID, int ally)
	{
		teams[teamID].lose = true;

		switch (GameplayManager.mode)
		{
		case Mode.Cooperative:
			if (alliesNumberOfStats[ally].teams.Contains(teamID))
			{
				alliesNumberOfStats[ally].teams.Remove (teamID);
			}

			if (alliesNumberOfStats[ally].teams.Count == 0)
			{
				if (ally == Allies)
				{
					winGame = false;
					loseGame = true;
				}
				else
				{
					winGame = true;
					loseGame = false;
				}
				
				SendMessage ("EndMatch");
			}
			break;

		case Mode.Deathmatch:
			loserTeams++;

			if (MyTeam == teamID)
			{
				loseGame = true;
			}

			if (loserTeams == numberOfTeams-1
				&& !loseGame)
			{
				winGame = true;
			}
			
			if (loserTeams == numberOfTeams-1)
			{
				SendMessage ("EndMatch");
			}
			break;

		default:
			break;
		}
	}

	void NoMainBase ()
	{
		hud.uiLostMainBaseObject.SetActive (false);

		CancelInvoke ("DecrementTime");
		photonView.RPC ("Defeat", PhotonTargets.All, MyTeam, Allies);
	}

	void DecrementTime ()
	{
		currentTime -= 1f;
		hud.labelTime.text = currentTime.ToString () + "s";
	}

	[RPC]
	void Defeat (int teamID, int ally)
	{
		if (teams[teamID].lose) return;

		ComponentGetter.Get<StatsController> ().DestroyAllStatsTeam (teamID);
		CheckCondition (teamID, ally);
	}

	void Update ()
	{
		hud.labelMana.text = resources.NumberOfMana.ToString ();
		hud.labelRocks.text = resources.NumberOfRocks.ToString ();
		hud.labelUnits.text = numberOfUnits.ToString () + "/" + maxOfUnits.ToString ();

		if (loseGame || winGame)
		{
			if (winGame)
			{
				hud.uiVictoryObject.SetActive (true);
				hud.uiDefeatObject.SetActive (false);
			}
			else
			{
				hud.uiVictoryObject.SetActive (false);
				hud.uiDefeatObject.SetActive (true);
			}
		}
	}

	void EndMatch ()
	{
		Debug.LogError ("EndMatch");

		Score.Save ();

		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();

		string encodedBattle = (string)pw.GetPropertyOnRoom ("battle");
		string encodedPlayer = (string)pw.GetPropertyOnPlayer ("player");

		if (!string.IsNullOrEmpty (encodedBattle) && !string.IsNullOrEmpty (encodedPlayer))
		{
			Model.Battle battle = new Model.Battle (encodedBattle);
			Model.Player player = new Model.Player(encodedPlayer);

			PlayerBattleDAO pbDAO = ComponentGetter.Get <PlayerBattleDAO> ();

			pbDAO.CreatePlayerBattle (player, battle,
			(playerBattle, message) =>
			{
				playerBattle.BlWin = winGame;

				pbDAO.UpdatePlayerBattle (playerBattle,
				(playerBattle_update, message_update) =>
				{
					if (playerBattle_update != null)
						Debug.Log ("message: " + message);
					else
						Debug.Log ("salvou playerBattle");
				});
			});
		}
	}
}
