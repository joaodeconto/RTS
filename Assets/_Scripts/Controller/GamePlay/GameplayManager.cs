using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Cloud.Analytics;
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
		
		colorTexture.SetPixel (0, 0, Color.white);
		
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
		public UILabel labelTotalTime;
		public GameObject uiVictoryObject;
		public GameObject uiDefeatObject;
//		public Transform buttonMatchScore;
		public GameObject uiWaitingPlayers;
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
		Cooperative,
		Survival,
		Tutorial
	}

	public static Mode mode;
	public bool pauseGame = false;
	private int readyCounter;
	private bool gamestarted = false;

	public const int MAX_POPULATION_ALLOWED = 200;
	public const int BOT_TEAM = 8;

	public Team[] teams;
	
	// Resources
	public ResourcesManager resources;
	
	public HUD hud;
	
	public int numberOfUnits { get; protected set; }
	public int TotalPopulation { get; protected set; }
	protected int numberOfActiveMainBases;

	private List<IHouse> houses = new List<IHouse> ();
	private Model.PlayerBattle playerBattle;
	
	protected List<AllyClass> alliesNumberOfStats = new List<AllyClass>();
	
	protected int loserTeams;
	protected int numberOfTeams;

	protected bool loseGame = false;
	protected bool winGame = false;
	protected float timeLeftToLoseTheGame;

	protected bool beingAttacked = false;

	public int MyTeam {get; protected set;}
	public int Allies {get; protected set;}

	public float myTimer = 0.0f;
	public int Triggerflag = 1;

	protected NetworkManager network;
	protected GameController gameController;


	public void Init ()
	{
		network = ComponentGetter.Get<NetworkManager>();
		gameController = ComponentGetter.Get<GameController> ();
	
		if (mode != Mode.Tutorial && !PhotonNetwork.offlineMode)
		{

			teams[8].initialPosition.gameObject.SetActive(false);
			GameObject tutorialC = GameObject.Find ("Tutorial Manager");
			tutorialC.SetActive (false);

			photonView.RPC ("MySceneReady", PhotonTargets.All);
			InvokeRepeating ("CheckGameStart",1f,1f);
			hud.uiWaitingPlayers.SetActive(true);

			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
			if (mode == Mode.Cooperative)
			{
				Allies = (int)PhotonNetwork.player.customProperties["allies"];
			}
			
			numberOfTeams = PhotonNetwork.room.maxPlayers;
			pauseGame = true;

		}

		else
		{
			MyTeam = 0;
			Allies = 0;
			gamestarted = true;
			gameController.GameStartInit();
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

	}

	void CheckGameStart()
	{
		int numberOfPlayers = PhotonNetwork.playerList.Length;
		if (numberOfPlayers >= readyCounter) photonView.RPC ("GameStart", PhotonTargets.All);
	}

	void MySceneReady()
	{
		readyCounter++;
	}

	void GameStart()
	{
		gamestarted = true;
		gameController.GameStartInit();
		hud.uiWaitingPlayers.SetActive(false);

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

	public bool IsBotTeam (IStats stats)
	{
		return stats.team == BOT_TEAM;
	}

	public bool IsAlly (int allyNumber)
	{
		return allyNumber == Allies;
	}

	public bool IsAlly (IStats stats)
	{
		return stats.ally == Allies;
	}

	public bool IsNotEnemy (int teamID, int ally)
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
					
					SoundSource ss = GetComponent<SoundSource> ();
					if (ss)
					{
						ss.Play ("BeingAttacked");
					}

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
			numberOfActiveMainBases++;
			hud.uiLostMainBaseObject.SetActive (false);
			CancelInvoke ("NoMainBase");
			CancelInvoke ("DecrementTime");
		}
	}

	public void DecrementMainBase (int teamID)
	{
		if (IsSameTeam (teamID))
		{
			numberOfActiveMainBases--;
			if (numberOfActiveMainBases == 0)
			{
				hud.uiLostMainBaseObject.SetActive (true);
				timeLeftToLoseTheGame = 40f;
				hud.labelTime.text = timeLeftToLoseTheGame.ToString () + "s";
				Invoke ("NoMainBase", timeLeftToLoseTheGame);
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

	public void AddHouse (IHouse house)
	{
		houses.Add (house);
		VerifyPopulation ();
	}
		
	public void VerifyPopulation ()
	{	
		int allowedPopulation = 0;
		
		foreach (IHouse house in houses)		
		{
			allowedPopulation += house.GetHousePopulation ();
			allowedPopulation  = Mathf.Min (allowedPopulation, MAX_POPULATION_ALLOWED);
		}
		
		TotalPopulation = allowedPopulation;
	}

	public void RemoveHouse (IHouse house)
	{
		houses.Remove (house);
		VerifyPopulation ();
	}

	public bool NeedMoreHouses (int additionalUnits)
	{
		return (numberOfUnits + additionalUnits > TotalPopulation);
	}

	public bool ReachedMaxPopulation
	{
		get
		{
			return (TotalPopulation >= MAX_POPULATION_ALLOWED);
		}
	}

	public void DefeatingEnemyTeamsByObjectives ()
	{
		if (PhotonNetwork.room.playerCount == 1)
		{
			winGame = true;
			EndMatch ();
		}
		else
		{
			for (int indexTeam = 0, maxTeams = teams.Length - 1; indexTeam != maxTeams; ++indexTeam)
			{
				if (indexTeam == MyTeam)
					continue;

				photonView.RPC ("DefeatOther", PhotonTargets.All, indexTeam);
			}
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
		timeLeftToLoseTheGame -= 1f;
		hud.labelTime.text = timeLeftToLoseTheGame.ToString () + "s";
	}

	[RPC]
	void DefeatOther (int teamID)
	{
		if (teamID == MyTeam)
		{
			photonView.RPC ("Defeat", PhotonTargets.All, MyTeam, Allies);
		}
	}

	[RPC]
	void Defeat (int teamID, int ally)
	{
		if (teams[teamID].lose) return;

		StatsController sc = ComponentGetter.Get<StatsController> ();
		
		sc.DestroyAllStatsTeam (teamID);
		
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
		case Mode.Tutorial:	
		case Mode.Deathmatch:
			loserTeams++;
			
			if (MyTeam == teamID)
			{
				loseGame = true;
			}
			
			//se o numero de times que perderam for o mesmo de times adversarios o jogador atual ganhou a partida
			if (loserTeams == numberOfTeams - 1 && !loseGame)
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

	void Update ()
	{
		if(gamestarted)
		{
		myTimer += Time.deltaTime;
		int minutes = Mathf.FloorToInt(myTimer / 60F);
		int seconds = Mathf.FloorToInt(myTimer - minutes * 60);
		string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
		hud.labelTotalTime.text = niceTime;
		}

		hud.labelMana.text = resources.Mana.ToString ();
		hud.labelRocks.text = resources.Rocks.ToString ();
		hud.labelUnits.text = numberOfUnits.ToString () + "/" + TotalPopulation.ToString ();

			
		if ((loseGame || winGame))
		{
//			Debug.Log ("hud.uiVictoryObject.SetActive (" + winGame + ") - hud.uiVictoryObject.SetActive (" + loseGame + ")");

			hud.uiVictoryObject.SetActive (winGame);
			hud.uiDefeatObject.SetActive (loseGame);
		
//			hud.buttonMatchScore.gameObject.SetActive (true);
//			
//			DefaultCallbackButton dcb = ComponentGetter.Get <DefaultCallbackButton> (hud.buttonMatchScore.transform, false);
//			
//			dcb.Init
//			(
//				null,
//				(ht_dcb) => 
//				{
//					if (PhotonNetwork.room != null)
//						PhotonNetwork.LeaveRoom ();
//					
//					Application.LoadLevel (0);
//				}
//			);
		}
	}

	public void EndMatch ()
	{
				
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		BidManager bm = ComponentGetter.Get <BidManager> ();
		
		if (winGame)
		{
			bm.WonTheGame ();
		}

		string encodedBattle = (string)pw.GetPropertyOnRoom ("battle");
		string encodedPlayer = (string)pw.GetPropertyOnPlayer ("player");

		if (!string.IsNullOrEmpty (encodedBattle) && !string.IsNullOrEmpty (encodedPlayer))
		{
			Model.Battle battle = new Model.Battle (encodedBattle);
			Model.Player player = new Model.Player(encodedPlayer);
			
			if (winGame)
			{
				Score.AddScorePoints (DataScoreEnum.Victory, 1, battle.IdBattle);
				Score.AddScorePoints (DataScoreEnum.Victory, 1);
			}
			else
			{
				Score.AddScorePoints (DataScoreEnum.Defeat, 1, battle.IdBattle);
				Score.AddScorePoints (DataScoreEnum.Defeat, 1);
			}

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
