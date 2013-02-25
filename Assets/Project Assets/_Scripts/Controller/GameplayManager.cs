using UnityEngine;
using System.Collections;

[System.Serializable]
public class Team
{
	public string name;
	public Color color = Color.white;
	public Texture2D colorTexture;
}

public class GameplayManager : MonoBehaviour {
	
	public Team[] teams;
	
	public int MyTeam {get; protected set;}
	
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
	}
	
	public Color GetColorTeam (int teamID)
	{
		for (int i = 0; i != teams.Length; i++)
		{
			if (i == teamID)
			{
				return teams[i].color;
			}
		}
		
		Debug.LogError ("Don't have Configuration Team for this Team ID.");
		return Color.black;
	}
	
	public bool IsSameTeam (Unit soldier)
	{
		return soldier.Team == MyTeam;
	}
	
	public bool IsSameTeam (FactoryBase factory)
	{
		return factory.Team == MyTeam;
	}
}
