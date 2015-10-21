using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Cloud.Analytics;
using Visiorama;
using I2.Loc;
using Visiorama.Utils;

[System.Serializable]
public class Team
{
	public string name;
	public Color[] colors = new Color[3] { Color.white, Color.gray, Color.blue };
	public Transform initialPosition;
	public bool lose { get; set; }
}

[System.Serializable]
public class Level
{
	public int levelId;
	public GameObject gameLevel;	
	public int startingRocks = 0;
	public int startingMana  = 0;
}

public class GameplayManager : Photon.MonoBehaviour
{
	#region Serializable e Declares
	[System.Serializable]
	public class HUD
	{    
		public UILabel    labelRocks;
		public UILabel    labelMana;
		public UILabel    labelUnits;
		public UILabel    labelTotalTime;
		public GameObject uiVictoryObject;
		public GameObject uiDefeatObject;
//		public Transform  buttonMatchScore;
		public GameObject uiWaitingPlayers;
		public UILabel    labelTime;
		public GameObject uiLostMainBaseObject;
		public GameObject pauseScreen;
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

	public const int MAX_POPULATION_ALLOWED = 200;
	public int TotalPopulation {get; protected set;}
	public int numberOfUnits {get; protected set;}
	public int MyTeam {get; protected set;}
	public int Allies {get; protected set;}
	public ResourcesManager resources;	
	public bool scoreCounting = true;
	public int clockPontuationLimit;
	public bool pauseGame = false;
	public const int BOT_TEAM = 8;
	public UILabel loadingMessage;
	public float gameTime = 0f;
	public Level selectedLevel;
	public static Mode mode;
	public Team[] teams;
	public Level[] level;
	public HUD hud;	

	protected List<AllyClass> alliesNumberOfStats = new List<AllyClass>();
	protected SelectionController selectionController;
	protected TouchController touchController;
	protected int numberOfActiveMainBases;
	protected float timeLeftToLoseTheGame;
	protected bool beingAttacked = false;
	protected NetworkManager network;
	protected bool loseGame = false;
	protected bool winGame = false;
	protected string encodedBattle;
	protected string encodedPlayer;
	protected StatsController sc;
	protected int numberOfTeams;
	protected int loserTeams;
	protected BidManager bm;
	protected Score score;

	private List<IHouse> houses = new List<IHouse> ();
	private int scenelevel = ConfigurationData.level;
	private Model.PlayerBattle playerBattle;
	private bool gamestarted = false;	
	private bool isPaused =false;
	private float pausedTime = 0;
	private int teamWhoPaused;
	private int readyCounter;
	#endregion

	#region Init e GameStart
	public void Init ()
	{	
		if (!PhotonNetwork.offlineMode){
		    score = ComponentGetter.Get <Score> ("$$$_Score");
			encodedBattle = ConfigurationData.battle.ToString();
			encodedPlayer = ConfigurationData.player.ToString();
		}
		hud.uiWaitingPlayers.SetActive(true);
		loadingMessage = hud.uiWaitingPlayers.GetComponentInChildren<UILabel>();
		touchController = ComponentGetter.Get<TouchController>();
		selectionController = ComponentGetter.Get<SelectionController>();
		sc = ComponentGetter.Get<StatsController> ();
		if (mode != Mode.Tutorial && !PhotonNetwork.offlineMode){
			selectedLevel = level[0];
			selectedLevel.gameLevel.SetActive(true);
			network = ComponentGetter.Get<NetworkManager>();
			bm = ComponentGetter.Get <BidManager> ();
			loadingMessage.text = ScriptLocalization.Get("Menus/synching tribes");
			gamestarted = false;
			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
			if (mode == Mode.Cooperative){
				Allies = (int)PhotonNetwork.player.customProperties["allies"];
			}
			numberOfTeams = PhotonNetwork.room.maxPlayers;
			pauseGame = false;
			StartCoroutine("TribeNetworkInstantiate");
		}

		else{
			foreach (Level lvl in level)
			{
				if (lvl.levelId == ConfigurationData.level){
					selectedLevel = lvl;
					selectedLevel.gameLevel.SetActive(true);
					break;
				}
			}
			PhotonNetwork.offlineMode = true;
			selectedLevel.gameLevel.GetComponent<TutorialManager>().Init();
			loadingMessage.text = ScriptLocalization.Get("Menus/Loading");
			MyTeam = 0;
			Allies = 0;
			numberOfTeams = 1;
			pauseGame = true;
			ComponentGetter.Get<OfflineScore> ().Init ();
			StartCoroutine ("TribeInstantiate");
		    selectedLevel.gameLevel.GetComponent<EnemyCluster> ().Init ();
		}

		if (mode == Mode.Cooperative){
			alliesNumberOfStats.Add (new AllyClass (0));
			alliesNumberOfStats.Add (new AllyClass (1));

			for (int i = alliesNumberOfStats.Count - 1; i != -1; --i)
			{
				foreach (PhotonPlayer pp in PhotonNetwork.playerList)
				{
					if (alliesNumberOfStats[i].ally == (int)pp.customProperties["allies"]){
						alliesNumberOfStats[i].teams.Add ((int)pp.customProperties["team"]);
					}
				}
			}
		}
		else loserTeams = 0;

		hud.uiDefeatObject.SetActive (false);
		hud.uiVictoryObject.SetActive (false);
		hud.uiLostMainBaseObject.SetActive (false);
	}

	void CheckGameStart()
	{
		if (readyCounter >= PhotonNetwork.playerList.Length){
			gamestarted = true;
			GameStart();
		}
	}

//	void TribeInstiateNetwork ()
//	{
//		for (int i = 0; i < 4; i++)
//		{
//			Team t = teams[i];
//			t.initialPosition = selectedLevel.gameLevel.transform.FindChild(i.ToString()).transform;
//			if(t.initialPosition != null && t.initialPosition.gameObject.activeSelf == true){
//				foreach (Transform trns in t.initialPosition)
//				{
//					if(trns.gameObject.activeSelf == true){
//						InitInstantiateNetwork toInit = trns.GetComponent<InitInstantiateNetwork>();
//						if (toInit.GetType() == typeof(InitInstantiateNetwork)){
//							toInit.Init();											
//						}
//					}
//				}
//			}
//		}
//	}
	IEnumerator TribeNetworkInstantiate ()
	{
		teams[MyTeam].initialPosition = selectedLevel.gameLevel.transform.FindChild(MyTeam.ToString()).transform;
		List<Transform> toInitList = new List<Transform>();
		foreach (Transform trns in teams[MyTeam].initialPosition)	
		{
			if(trns.gameObject.activeSelf == true)	toInitList.Add(trns);
		}
		
		for (int i = toInitList.Count -1; i >= 0; i--)
		{	
			InitInstantiateNetwork toInit = toInitList[i].GetComponent<InitInstantiateNetwork>();
			if (toInit != null){
				toInit.Init();											
				yield return new WaitForSeconds(0.1f);
			}
		}
		Invoke("CallMySceneReady",1f);
	}

	IEnumerator TribeInstantiate ()
	{
		teams[0].initialPosition = selectedLevel.gameLevel.transform.FindChild("0").transform;
		List<Transform> toInitList = new List<Transform>();
		foreach (Transform trns in teams[0].initialPosition)	
		{
			if(trns.gameObject.activeSelf == true)	toInitList.Add(trns);
		}

		for (int i = toInitList.Count -1; i >= 0; i--)
		{	
			InitInstantiateNetwork toInit = toInitList[i].GetComponent<InitInstantiateNetwork>();
			if (toInit != null){
				toInit.Init();											
				yield return new WaitForSeconds(0.1f);
			}
		}
		Invoke("GameStart",0.5f);
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
			if (MyTeam == i){
				Math.CenterCameraInObject (Camera.main, teams[i].initialPosition.position);
			}
		}
		Loading ld = hud.uiWaitingPlayers.GetComponent<Loading>();
		ld.reverseAlpha();
		resources.DeliverResources (Resource.Type.Rock, selectedLevel.startingRocks);		
		resources.DeliverResources (Resource.Type.Mana, selectedLevel.startingMana);
	}
	#endregion

	#region Update
	void Update ()
	{
		if(gamestarted){	
			if(scoreCounting){				
				hud.labelMana.text = resources.Mana.ToString ();
				hud.labelRocks.text = resources.Rocks.ToString ();
				hud.labelUnits.text = numberOfUnits.ToString () + "/" + TotalPopulation.ToString ();
				ClockGameplay();
			}
		}
		else CheckGameStart();
		
		if ((loseGame || winGame)){
			hud.uiVictoryObject.SetActive (winGame);
			hud.uiDefeatObject.SetActive (loseGame);		
		}

//		if(!PhotonNetwork.offlineMode && isPaused)
//		{
//			if((pausedTime + 30f)< Time.unscaledTime)
//			{
//				photonView.RPC("DefeatOther", PhotonTargets.OthersBuffered,teamWhoPaused);
//				GamePaused(false);
//			}
//		}
	}
	#endregion
		
	#region Get Colors

	public Color GetColorTeam (){return GetColorTeam (MyTeam, 0);}
	public Color GetColorTeam (int teamID) { return GetColorTeam (teamID, 0); }
	public Color GetColorTeam (int teamID, int indexColor)
	{
		if (teamID >= 0 && teamID < teams.Length){
			if (indexColor >= 0 && indexColor < teams[teamID].colors.Length){
				return teams[teamID].colors[indexColor];
			}
			else{
				Debug.LogError ("The Team does not have the indexColor " + indexColor);
				return Color.black;
			}
		}
		else{
			Debug.LogError ("Team ID does not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
			return Color.black;
		}
	}
	#endregion

	#region Check Teams e Stats
	public bool IsSameTeam (int teamID){return teamID == MyTeam;}
	public bool IsSameTeam (IStats stats){return stats.team == MyTeam;}
	public bool IsBotTeam (IStats stats){return stats.team == BOT_TEAM;}
	public bool IsAlly (int allyNumber){return allyNumber == Allies;}
	public bool IsAlly (IStats stats){return stats.ally == Allies;}
	public bool IsNotEnemy (int teamID, int ally)
	{
		if (GameplayManager.mode == Mode.Cooperative){
			return IsAlly (ally);
		}
		else{
			return IsSameTeam (teamID);
		}
	}
	#endregion

	#region Being Attacked
	public bool IsBeingAttacked (IStats target)
	{
		if (!beingAttacked){
			if (IsSameTeam (target)){
				bool isInCamera = ComponentGetter.Get<TouchController> ().IsInCamera (target.transform.position);
				if (!isInCamera){
					beingAttacked = true;
					Invoke ("BeingAttackedToFalse", 5f);					
					SoundSource ss = GetComponent<SoundSource> ();
					if (ss)	ss.Play ("BeingAttacked");
					return true;
				}
			}
		}
		return false;
	}

	void BeingAttackedToFalse (){beingAttacked = false;}
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
		get{
			return (TotalPopulation >= MAX_POPULATION_ALLOWED);
		}
	}
	#endregion

	#region MainBase Methods

	public void IncrementMainBase (int teamID)
	{
		if (IsSameTeam (teamID)){
			numberOfActiveMainBases++;
			hud.uiLostMainBaseObject.SetActive (false);
			CancelInvoke ("NoMainBase");
			CancelInvoke ("DecrementTime");
		}
	}
	
	public void DecrementMainBase (int teamID)
	{
		if (IsSameTeam (teamID)){
			numberOfActiveMainBases--;
			if (numberOfActiveMainBases == 0 && (!loseGame && !winGame)){
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
		if (sc.myStats.Count == 0){
			CancelInvoke("NoMainbase");
			NoMainBase();
		}
		else{
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
		if(state){
			touchController.mainCamera.GetComponent<CameraMovement>().enabled = false;
			selectionController.enabled = false;
			if(!PhotonNetwork.offlineMode) hud.pauseScreen.SetActive(true);
			Time.timeScale = 0.0f;
			isPaused = true;
		}
		else{
			touchController.mainCamera.GetComponent<CameraMovement>().enabled = true;
			selectionController.enabled = true;
			if(!PhotonNetwork.offlineMode)hud.pauseScreen.SetActive(false);
			Time.timeScale = 1.0f;
			isPaused = false;
		}
	}

	#endregion
	
	#region EndMatch
	public void EndMatch ()
	{
		if (!PhotonNetwork.offlineMode){
			if (winGame)	bm.WonTheGame ();
			Model.Battle battle = new Model.Battle (encodedBattle);
			Model.Player player = new Model.Player(encodedPlayer);

			if(scoreCounting){				
				if (winGame)	Score.AddScorePoints (DataScoreEnum.Victory, 1, battle.IdBattle);
				else			Score.AddScorePoints (DataScoreEnum.Defeat, 1, battle.IdBattle);
				Score.AddScorePoints (DataScoreEnum.TotalTimeElapsed, (int)gameTime, battle.IdBattle);
				score.SaveScore();
				scoreCounting = false;			
			}

			PlayerBattleDAO pbDAO = ComponentGetter.Get <PlayerBattleDAO> ();
			pbDAO.CreatePlayerBattle (player, battle,
			(playerBattle, message) =>{
				playerBattle.BlWin = winGame;
				pbDAO.UpdatePlayerBattle (playerBattle,
				(playerBattle_update, message_update) =>{

				});
			});
		}
		else
		{
			if (PhotonNetwork.offlineMode && scoreCounting){
				OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
				if (winGame)	oScore.oPlayers[0].AddScorePlayer(DataScoreEnum.Victory, 1);
				else			oScore.oPlayers[0].AddScorePlayer(DataScoreEnum.Defeat, 1);				
				oScore.oPlayers[0].AddScorePlayer (DataScoreEnum.TotalTimeElapsed, (int)gameTime);
			}
		}
		if (!PhotonNetwork.offlineMode) PhotonNetwork.LeaveRoom ();
	}

	public void Surrender ()
	{	
		if(!winGame && !loseGame)	Defeat(MyTeam, Allies);
	}

	public void DefeatingEnemyTeamsByObjectives ()
	{		
		winGame = true;
		
		if (PhotonNetwork.offlineMode){
			EndMatch ();
			return;
		}
		
		if (PhotonNetwork.room.playerCount == 1){
			EndMatch ();
		}
		
		else{
			for (int indexTeam = 0, maxTeams = teams.Length - 1; indexTeam != maxTeams; ++indexTeam)
			{
				if (indexTeam == MyTeam)	continue;
				photonView.RPC ("DefeatOther", PhotonTargets.All, indexTeam);
			}
		}
	}
	#endregion

	#region RPC's
	[RPC]
	void DefeatOther (int teamID)
	{
		if (teamID == MyTeam)	photonView.RPC ("Defeat", PhotonTargets.All, MyTeam, Allies);
	}
	
	[RPC]
	public void Defeat (int teamID, int ally)
	{
		selectedLevel.gameLevel.GetComponent<VictoryCondition>().InactiveAllChallenges();
		if (teams[teamID].lose) return;				
		teams[teamID].lose = true;			
		switch (GameplayManager.mode){
		case Mode.Cooperative:
			if (alliesNumberOfStats[ally].teams.Contains(teamID)){
				alliesNumberOfStats[ally].teams.Remove (teamID);
			}
			
			if (alliesNumberOfStats[ally].teams.Count == 0){
				if (ally == Allies){
					winGame = false;
					loseGame = true;
				}
				else{
					winGame = true;
					loseGame = false;
				}				
				SendMessage ("EndMatch");
			}
			break;
		case Mode.Tutorial:			
			if (MyTeam == teamID){
				loseGame = true;
				selectedLevel.gameLevel.GetComponent<EnemyCluster>().enabled = false;
				loserTeams++;
				SendMessage ("EndMatch");
				ComponentGetter.Get<OfflineScore> ().oPlayers[8].AddScorePlayer(DataScoreEnum.Victory, 1);
				ComponentGetter.Get<OfflineScore> ().oPlayers[0].AddScorePlayer(DataScoreEnum.TotalTimeElapsed, (int)gameTime);
				scoreCounting = false;
				Invoke("DestroyAllStats", 1f);
			}
			break;

		case Mode.Deathmatch:
			if (MyTeam == teamID){
				loseGame = true;		
				Model.Battle battle = ConfigurationData.battle;
				if (scoreCounting){
					Score.AddScorePoints (DataScoreEnum.Defeat, 1, battle.IdBattle);					
					Score.AddScorePoints (DataScoreEnum.TotalTimeElapsed, (int)gameTime, battle.IdBattle);					
					score.SaveScore();
				}
				scoreCounting = false;
			}

			sc.DestroyAllStatsTeam (teamID);			
			loserTeams++;			
			if (loserTeams == numberOfTeams - 1 && !loseGame)	winGame = true;						
			if (loserTeams == numberOfTeams-1)	SendMessage ("EndMatch");
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

//	IEnumerator OnApplicationFocus(bool running)
//	{
//		if(!PhotonNetwork.offlineMode){		 
//			if(!running){
//				photonView.RPC("ApplicationPaused",PhotonTargets.Others, true, MyTeam);
//				GamePaused (true);
//			}
//			else{
//				photonView.RPC("ApplicationPaused",PhotonTargets.Others, false, MyTeam);
//				GamePaused (false);
//			}
//			yield return new WaitForSeconds(1f);
//		}
//	}
	
	[RPC]
	void MySceneReady()
	{
		readyCounter++;
	}

//	[RPC]
//	void ApplicationPaused(bool state, int teamPaused)
//	{
//		GamePaused(state);
//		if(isPaused && teamWhoPaused != MyTeam){
//			pausedTime = Time.unscaledTime;
//			teamWhoPaused = teamPaused;
//		}		
//	}
	#endregion
}
