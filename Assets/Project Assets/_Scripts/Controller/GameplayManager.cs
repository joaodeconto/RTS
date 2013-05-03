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
	
	protected Dictionary<int, int> teamNumberOfStats = new Dictionary<int, int>();
	protected int[][] alliesNumberOfStats;
	protected int loserTeams;
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
			Allies = (int)PhotonNetwork.player.customProperties["allies"];
			
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
//			alliesNumberOfStats = new int[2][];
//			
//			for (int i = alliesNumberOfStats.Length - 1; i != -1; --i)
//			{
//				alliesNumberOfStats[i] = PhotonNetwork.playerList.Length / 2;
//				foreach (PhotonPlayer pp in PhotonNetwork.playerList)
//				{
//					alliesNumberOfStats[pp.customProperties["allies"]][pp.customProperties["team"]] = 0;
//				}
//			}
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
	
	public bool IsBeingAttacked (IStats target)
	{
		if (!beingAttacked)
		{
			if (IsSameTeam (target))
			{
				Vector3 pos = Camera.mainCamera.WorldToViewportPoint (target.transform.position);
				
				bool isInCamera = (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
				
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
	
	public void AddStatTeamID (int teamID)
	{
		if (teamNumberOfStats.ContainsKey(teamID))
		    teamNumberOfStats[teamID] = teamNumberOfStats[teamID] + 1;
		else
			teamNumberOfStats.Add (teamID, 1);
	}
	
	public void AddStatTeamID (int ally, int teamID)
	{
		alliesNumberOfStats[ally][teamID] += 1;
	}

	public void RemoveStatTeamID (int teamID)
	{
		if (teamNumberOfStats.ContainsKey(teamID))
		    teamNumberOfStats[teamID] = teamNumberOfStats[teamID] - 1;
		else
			return;

		CheckCondition (teamID);
	}
	
	public void RemoveStatTeamID (int ally, int teamID)
	{
		alliesNumberOfStats[ally][teamID] -= 1;
	}

	public void RemoveAllStats (int teamID)
	{
		if (teamNumberOfStats.ContainsKey(teamID))
		{
			if (teamNumberOfStats[teamID] == 0)
			{
				return;
			}
			else
			{
			    teamNumberOfStats[teamID] = 0;
			}
		}
		else
			return;

		CheckCondition (teamID);
	}
	
	public void RemoveAllStats (int ally, int teamID)
	{
		alliesNumberOfStats[ally][teamID] = 0;
	}

	public void CheckCondition (int teamID)
	{
		if (teamNumberOfStats.ContainsKey(teamID))
		{
			if (teamNumberOfStats[teamID] == 0)
			{
				loserTeams++;
				if (MyTeam == teamID)
				{
					loseGame = true;
					return;
				}
			}
			else
				return;
		}
		else
		{
			return;
		}

		if (loserTeams == teamNumberOfStats.Count-1
			&& !loseGame)
		{
			winGame = true;
		}
	}
	
	public void CheckCondition (int ally, int teamID)
	{
		if (alliesNumberOfStats[ally][teamID] == 0)
		{
			alliesNumberOfStats[ally][0] -= 1;
		}
		
		if (alliesNumberOfStats[ally][0] == 0)
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
	}
	
	void NoMainBase ()
	{
		hud.uiLostMainBaseObject.SetActive (false);
		
		CancelInvoke ("DecrementTime");
		photonView.RPC ("NewLoser", PhotonTargets.All, MyTeam);
	}
	
	void DecrementTime ()
	{
		currentTime -= 1f;
		hud.labelTime.text = currentTime.ToString () + "s";
	}
	
	[RPC]
	void NewLoser (int teamID)
	{
		ComponentGetter.Get<TroopController> ().DestroySoldiersTeam (teamID);
		ComponentGetter.Get<FactoryController> ().DestroyFactorysTeam (teamID);
		CheckCondition (teamID);
	}

	// TODO: Mostrando só os valores na tela
	void Update ()
	{
		hud.labelResources.text = resources.NumberOfRocks.ToString ();
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
//			enabled = false;
		}
	}

	void EndGame ()
	{
//		network.photonView.RPC ("ChangeLevel", PhotonTargets.All, 0);
	}
}
