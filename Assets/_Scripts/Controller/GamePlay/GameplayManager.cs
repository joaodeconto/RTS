using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Cloud.Analytics;
using UnityEngine.Advertisements;
using Visiorama;
using Visiorama.Utils;

[System.Serializable]
public class Team
{
	public string name;
	public Color[] colors = new Color[3] { Color.white, Color.gray, Color.blue };
	public Transform initialPosition;
	public bool lose { get; set; }
}

public class GameplayManager : Photon.MonoBehaviour
{
	#region Serializable e Declares
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

	public int startingRocks = 0;
	public static Mode mode;
	public bool pauseGame = false;
	private int readyCounter;
	private bool gamestarted = false;
	public float gameTime = 0f;
	public const int MAX_POPULATION_ALLOWED = 200;
	public const int BOT_TEAM = 8;
	public Team[] teams;
	public ResourcesManager resources;	
	public HUD hud;	
	public int numberOfUnits {get; protected set;}
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
	public bool scoreCounting = true;
	public int clockPontuationLimit;
	protected NetworkManager network;
	protected BidManager bm;
	protected TouchController touchController;
	protected SelectionController selectionController;
	protected StatsController sc;
	public UILabel loadingMessage;
	protected Score score;
	protected string encodedBattle;
	protected string encodedPlayer;

	#endregion

	#region Init e GameStart
	public void Init ()
	{
	
		if (!PhotonNetwork.offlineMode)
		{
		    score = ComponentGetter.Get <Score> ("$$$_Score");
			encodedBattle = ConfigurationData.battle.ToString();
			encodedPlayer = ConfigurationData.player.ToString();
		}
		hud.uiWaitingPlayers.SetActive(true);
		loadingMessage = hud.uiWaitingPlayers.GetComponentInChildren<UILabel>();
		touchController = ComponentGetter.Get<TouchController>();
		selectionController = ComponentGetter.Get<SelectionController>();
		sc = ComponentGetter.Get<StatsController> ();

		if (mode != Mode.Tutorial && !PhotonNetwork.offlineMode)
		{
			network = ComponentGetter.Get<NetworkManager>();
			bm = ComponentGetter.Get <BidManager> ();
			loadingMessage.text = "synching tribes";
			gamestarted = false;

			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
			if (mode == Mode.Cooperative)
			{
				Allies = (int)PhotonNetwork.player.customProperties["allies"];
			}
			numberOfTeams = PhotonNetwork.room.maxPlayers;
			pauseGame = false;
			TribeInstiateNetwork();
		}

		else
		{
			PhotonNetwork.offlineMode = true;
			GameObject tutorialC = GameObject.Find ("Components/Tutorial Manager");
			if (tutorialC)	tutorialC.SetActive (true);
			loadingMessage.text = "loading";
			MyTeam = 0;
			Allies = 0;
			numberOfTeams = 1;
			pauseGame = true;
			TribeInstiate ();
			ComponentGetter.Get<EnemyCluster> ().Init ();
			Invoke("GameStart",2f);
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
		loadingMessage.text = "loading";
		if (mode != Mode.Tutorial && !PhotonNetwork.offlineMode) Invoke("CallMySceneReady",1f);
	}

	void CheckGameStart()
	{
		int numberOfPlayers = PhotonNetwork.playerList.Length;
		if (readyCounter >= numberOfPlayers)
		{		
			gamestarted = true;
			GameStart();
		}
	}

	void TribeInstiateNetwork ()
	{
		
		Debug.Log ("Tribe Network");
		foreach (Team t in teams)
		{
			if(t.initialPosition != null && t.initialPosition.gameObject.activeSelf == true && t.name != "selvagens")
			{
				foreach (Transform trns in t.initialPosition)
				{
					if(trns.gameObject.activeSelf == true)
					{
						InitInstantiateNetwork toInit = trns.GetComponent<InitInstantiateNetwork>();
						if (toInit.GetType() == typeof(InitInstantiateNetwork))
						{
							toInit.Init();											
						}
					}
				}
			}
		}
	}

	void TribeInstiate ()
	{
		Debug.Log ("Tribe Instantiate");
		foreach (Transform trns in teams[0].initialPosition)
		{
			if(trns.gameObject.activeSelf == true)
			{
				InitInstantiateNetwork toInit = trns.GetComponent<InitInstantiateNetwork>();
				if (toInit.GetType() == typeof(InitInstantiateNetwork))
				{
					toInit.Init();											
				}
			}
		}
			

	}

	void CallMySceneReady()
	{
		photonView.RPC ("MySceneReady", PhotonTargets.AllBuffered);
	}

	void GameStart()
	{
		gamestarted = true;

		for (int i = 0; i != teams.Length; i++)
		{
			if (MyTeam == i)
			{
				Math.CenterCameraInObject (Camera.main, teams[i].initialPosition.position);
			}
		}
		GamePaused(false);
		Loading ld = hud.uiWaitingPlayers.GetComponent<Loading>();
		ld.reverseAlpha();
		resources.DeliverResources (Resource.Type.Rock, startingRocks);
	}
	#endregion

	#region Update
	void Update ()
	{
		if(gamestarted)
		{	
			if(scoreCounting)
			{				
				hud.labelMana.text = resources.Mana.ToString ();
				hud.labelRocks.text = resources.Rocks.ToString ();
				hud.labelUnits.text = numberOfUnits.ToString () + "/" + TotalPopulation.ToString ();
				ClockGameplay();
			}
		}
		else CheckGameStart();
		
		if ((loseGame || winGame))
		{
			hud.uiVictoryObject.SetActive (winGame);
			hud.uiDefeatObject.SetActive (loseGame);		
		}
	}
	#endregion
		
	#region Get Colors
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
	#endregion

	#region Check Teams e Stats
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
	#endregion

	#region Being Attacked

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
	#endregion

	#region Population Methods

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
		winGame = true;

		if (PhotonNetwork.offlineMode) return;

		if (PhotonNetwork.room.playerCount == 1)
		{
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

	#endregion

	#region MainBase Methods

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
			if (numberOfActiveMainBases == 0 && (!loseGame && !winGame))
			{
				hud.uiLostMainBaseObject.SetActive (true);
				timeLeftToLoseTheGame = 40f;
				hud.labelTime.text = timeLeftToLoseTheGame.ToString () + "s";
				Invoke ("NoMainBase", timeLeftToLoseTheGame);
				InvokeRepeating ("DecrementTime", 1f, 1f);
			}
		}
	}

	void NoMainBase ()
	{
		hud.uiLostMainBaseObject.SetActive (false);
		CancelInvoke ("DecrementTime");
		if (!PhotonNetwork.offlineMode) photonView.RPC ("Defeat", PhotonTargets.All, MyTeam, Allies);
		else Defeat(MyTeam, Allies);
	}

	void DecrementTime ()
	{
		if (sc.myStats.Count == 0)
		{
			CancelInvoke("NoMainbase");
			NoMainBase();
		}
		else
		{
			timeLeftToLoseTheGame -= 1f;
			hud.labelTime.text = timeLeftToLoseTheGame.ToString () + "s";
		}
	}
	#endregion

	#region Gamepause e Clock
	void ClockGameplay()
	{
		gameTime += Time.deltaTime;
		int minutes = Mathf.FloorToInt(gameTime / 60F);
		int seconds = Mathf.FloorToInt(gameTime - minutes * 60);
		string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
		hud.labelTotalTime.text = niceTime;
	}

	public void GamePaused(bool state)
	{
		if(state)
		{
			touchController.mainCamera.GetComponent<CameraMovement>().enabled = false;
			selectionController.enabled = false;
			Time.timeScale = 0.0f;
		}
		else
		{
			touchController.mainCamera.GetComponent<CameraMovement>().enabled = true;
			selectionController.enabled = true;
			Time.timeScale = 1.0f;
		}
	}
	#endregion
	
	#region EndMatch
	public void EndMatch ()
	{
		if(ConfigurationData.addPass || ConfigurationData.multiPass) {}
		else Advertisement.Show(null, new ShowOptions{pause = true,resultCallback = result => {}});

		if (!PhotonNetwork.offlineMode)
		{
			if (winGame)
			{
				bm.WonTheGame ();
			}

			Model.Battle battle = new Model.Battle (encodedBattle);
			Model.Player player = new Model.Player(encodedPlayer);

			if(scoreCounting)
			{				
				if (winGame)	Score.AddScorePoints (DataScoreEnum.Victory, 1, battle.IdBattle);
				else			Score.AddScorePoints (DataScoreEnum.Defeat, 1, battle.IdBattle);

				Score.AddScorePoints (DataScoreEnum.TotalTimeElapsed, (int)gameTime, battle.IdBattle);
				score.SaveScore();
				scoreCounting = false;			
			}

			PlayerBattleDAO pbDAO = ComponentGetter.Get <PlayerBattleDAO> ();
			pbDAO.CreatePlayerBattle (player, battle,
			(playerBattle, message) =>
			{
				playerBattle.BlWin = winGame;
				pbDAO.UpdatePlayerBattle (playerBattle,
				(playerBattle_update, message_update) =>
				{

				});
			});
		}

		if (!PhotonNetwork.offlineMode) PhotonNetwork.LeaveRoom ();
	}

	public void Surrender ()
	{	
		if(!winGame && !loseGame)	Defeat(MyTeam, Allies);
	}
	#endregion

	#region RPC's
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
		ComponentGetter.Get<VictoryCondition>().InactiveAllChallenges();

		if (teams[teamID].lose) return;
				
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
			
			if (MyTeam == teamID)
			{
				loseGame = true;
				scoreCounting = false;
				ComponentGetter.Get<EnemyCluster>().enabled = false;
				loserTeams++;	
				Invoke("DestroyAllStats", 1f);
				SendMessage ("EndMatch");
			}

			break;

		case Mode.Deathmatch:

			if (MyTeam == teamID)
			{

				loseGame = true;		
				Model.Battle battle = ConfigurationData.battle;
				if (scoreCounting)
				{
					Score.AddScorePoints (DataScoreEnum.Defeat, 1, battle.IdBattle);					
					Score.AddScorePoints (DataScoreEnum.TotalTimeElapsed, (int)gameTime, battle.IdBattle);					
					score.SaveScore();
				}

				scoreCounting = false;

			}

			sc.DestroyAllStatsTeam (teamID);			
			loserTeams++;			

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
	void DestroyAllStats()
	{
		sc.DestroyAllStatsTeam (MyTeam);
		sc.DestroyAllStatsTeam (BOT_TEAM);
	}
	
	[RPC]
	void MySceneReady()
	{
		readyCounter++;
	}
	#endregion
}
