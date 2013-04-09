using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Team
{
	public string name;
	public Color color = Color.white;
	public Texture2D colorTexture;
	public Transform initialPosition;
}

public class GameplayManager : MonoBehaviour
{
	public const int MAX_POPULATION_ALLOWED = 200;
	
	public Team[] teams;

	public int numberOfUnits { get; protected set; }
	public int maxOfUnits { get; protected set; }
	protected int excessHousesIncrements;
	
	protected Dictionary<int, int> teamNumberOfStats = new Dictionary<int, int>();
	protected int loserTeams;
	protected bool loseGame = false;
	protected bool winGame = false;

	public int MyTeam {get; protected set;}

	// Resources
	public ResourcesManager resources;

	public void Init ()
	{
		if (!PhotonNetwork.offlineMode)
		{
			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			MyTeam = 0;
		}

		for (int i = 0; i != teams.Length; i++)
		{
			if (MyTeam == i)
			{
				Camera.mainCamera.transform.position = teams[i].initialPosition.position;
			}
		}
		excessHousesIncrements = 0;
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
		if (teamID >= 0 && teamID < teams.Length) return teams[teamID].color;
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
		if (!ReachedMaxPopulation) maxOfUnits += numberOfIncrementUnits;
		else ++excessHousesIncrements;
	}

	public void DecrementMaxOfUnits (int numberOfDecrementUnits)
	{
		if (excessHousesIncrements == 0) maxOfUnits -= numberOfDecrementUnits;
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

	public void RemoveStatTeamID (int teamID)
	{
		if (teamNumberOfStats.ContainsKey(teamID))
		    teamNumberOfStats[teamID] = teamNumberOfStats[teamID] - 1;
		else
			return;

		CheckCondition (teamID);
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

	// TODO: Mostrando só os valores na tela
	void OnGUI ()
	{
		GUI.Box (new Rect(10, 10, 150, 25), "Resources: " + resources.NumberOfRocks.ToString ());
		GUI.Box (new Rect(10, 35, 150, 25), "Units: " + numberOfUnits.ToString () + "/" + maxOfUnits.ToString ());

		if (loseGame) GUI.Box (new Rect(Screen.width/2 - 75, Screen.height/2 - 12, 150, 25), "LOSER! ):");
		else if (winGame) GUI.Box (new Rect(Screen.width/2 - 75, Screen.height/2 - 12, 150, 25), "WIN! :D");
	}

	void EndGame ()
	{
		Visiorama.ComponentGetter.Get<NetworkManager>().photonView.RPC ("ChangeLevel", PhotonTargets.All, 0);
	}
}
