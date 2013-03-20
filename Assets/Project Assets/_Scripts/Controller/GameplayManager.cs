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
	
	public Team[] teams;
	
	public int maxOfUnits = 10;
	
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
	}
	
	public Color GetColorTeam (int teamID)
	{
		if (teamID >= 0 && teamID < teams.Length) return teams[teamID].color;
		else
		{
			Debug.LogError ("Team ID not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
			return Color.black;
		}
	}
	
	public bool IsSameTeam (int team)
	{
		return team == MyTeam;
	}
	
	public bool IsSameTeam (Unit soldier)
	{
		return soldier.Team == MyTeam;
	}
	
	public bool IsSameTeam (FactoryBase factory)
	{
		return factory.Team == MyTeam;
	}
	
	public void IncrementMaxOfUnits ()
	{
		maxOfUnits += 5;
	}
	
	public void DecrementMaxOfUnits ()
	{
		maxOfUnits -= 5;
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
//		GUI.Box (new Rect(10, 35, 150, 25), "Units: " + teamNumberOfStats[MyTeam].ToString () + "/" + maxOfUnits.ToString ());
		
		if (loseGame) GUI.Box (new Rect(Screen.width/2 - 75, Screen.height/2 - 12, 150, 25), "LOSER! ):"); 
		else if (winGame) GUI.Box (new Rect(Screen.width/2 - 75, Screen.height/2 - 12, 150, 25), "WIN! :D");
	}
	
	void EndGame ()
	{
		Visiorama.ComponentGetter.Get<NetworkManager>().photonView.RPC ("ChangeLevel", PhotonTargets.All, 0);
	}
}