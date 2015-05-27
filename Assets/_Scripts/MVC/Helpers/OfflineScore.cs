using UnityEngine;
using System.Collections.Generic;
using Visiorama;

public class OfflineScore : MonoBehaviour
{	
	protected GameplayManager gameplayManager;
	public Dictionary<int, ScorePlayer> oPlayers = new Dictionary<int, ScorePlayer>();

	public  void Init ()
	{
		DontDestroyOnLoad(transform.gameObject);
		gameplayManager = ComponentGetter.Get<GameplayManager>();
		int myPlayerId = gameplayManager.MyTeam;
		foreach (Team t in gameplayManager.teams)
		{
			if (t.initialPosition != null && (t.initialPosition.name == "0" || t.initialPosition.name == "8"))
			{
				string teamString = t.initialPosition.name;
				int teamInt = int.Parse(teamString);
				oPlayers.Add(teamInt, new ScorePlayer());
			}
		}
	}

	public void DestroyMe()
	{
		Destroy(this.gameObject);
	}
}