using UnityEngine;
using System.Collections;

public class GameplayManager : MonoBehaviour {
	
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
