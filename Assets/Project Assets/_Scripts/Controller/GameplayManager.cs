using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

[System.Serializable]
public class Team
{
	public string name;
	public Color color = Color.white;
	public Texture2D colorTexture;
	public Transform initialPosition;

	public bool lose { get; set; }
}

public class GameplayManager : Photon.MonoBehaviour
{
	[System.Serializable]
	public class HUD
	{
		public UILabel labelResources;
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
		Normal,
		Allies
	}

	public static Mode mode;

	public const int MAX_POPULATION_ALLOWED = 200;

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
			if (mode == Mode.Allies)
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
				Camera.mainCamera.transform.position = teams[i].initialPosition.position;
			}
		}

		if (mode == Mode.Allies)
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
	}

	/// <summary>
	/// Gets the color of my team.
	/// </summary>
	/// <returns>
	/// The of my color team.
	/// </returns>
	public Color GetColorTeam ()
	{
		return GetColorTeam (MyTeam);
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
	public Color GetColorTeam (int teamID)
	{
		if (teamID >= 0 && teamID < teams.Length)
		{
			return teams[teamID].color;
		}
		else
		{
			Debug.LogError ("Team ID not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
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
		return team == 8;
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
		if (GameplayManager.mode == Mode.Allies)
			return IsAlly (ally);
		else
			return IsSameTeam (teamID);
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
					Invoke ("BeingAttackedToFalse", 10f);
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
		case Mode.Allies:
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
			}
			break;

		case Mode.Normal:
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

	//TODO retirar tosqueira cristian! :D
	bool cabou = false;

	// TODO: Mostrando só os valores na tela
	void Update ()
	{
		if (cabou) return;

		hud.labelResources.text = resources.NumberOfRocks.ToString ();
		hud.labelUnits.text = numberOfUnits.ToString () + "/" + maxOfUnits.ToString ();

		if (loseGame || winGame)
		{
			cabou = true;

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

			PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();

			//TODO CRISTIAN BOTAR ISSO NO MÉTODO ENDIGUEIME @_@
			Model.Battle battle = new Model.Battle ((string)pw.GetPropertyOnRoom ("battle"));
			Model.Player player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));

			PlayerBattleDAO pbDAO = ComponentGetter.Get <PlayerBattleDAO> ();

			pbDAO.CreatePlayerBattle (player, battle,
			(dbPlayerBattle, response) =>
			{
				dbPlayerBattle.BlWin = winGame ? 1 : 0;

				pbDAO.UpdatePlayerBattle (dbPlayerBattle,
				(dbPlayerBattle_updated, message) =>
				{
					if (dbPlayerBattle_updated != null)
					{
						Debug.Log ("message: " + message);
					}
					else
					{
						Debug.Log ("salvou playerBattle");
					}
				});
			});

//			enabled = false;
		}
	}

	void EndGame ()
	{
//		network.photonView.RPC ("ChangeLevel", PhotonTargets.All, 0);
	}
}
