using UnityEngine;
using System.Collections;

[System.Serializable]
public class Team
{
	public string name;
	public Color color;
	public Texture2D colorTexture;
}

public class GameplayManager : MonoBehaviour {
	
	public Team[] team;
	
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
	
	public bool IsSameTeam (Unit soldier)
	{
		return soldier.Team == MyTeam;
	}
	
	public bool IsSameTeam (FactoryBase factory)
	{
		return factory.Team == MyTeam;
	}
}
