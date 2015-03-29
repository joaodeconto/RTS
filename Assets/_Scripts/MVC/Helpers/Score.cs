using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using Visiorama;

public struct DataScoreEnum
{
	public const string CurrentCrystals		= "current-crystals";
	public const string TotalCrystals 		= "total-crystals";	
	public const string Victory		        = "victory";
	public const string Defeat  			= "defeat";
	public const string TotalTimeElapsed  	= "time-elapsed";
	public const string ResourcesGathered 	= "Resources gathered";
	
	public const string UnitsCreated 		= "units-created";
	public const string BuildingsCreated	= "buildings-created";
	public const string UpgradesCreated		= "upgrades-created";
	public const string UnitsLost 			= "units-lost";
	public const string BuildingsLost 		= "buildings-lost";	
	public const string UnitsKilled 		= "units-killed";
	public const string BuildingsDestroyed  = "buildings-destroyed";

	public const string XUpgraded			= "-created";
	public const string XBuilt				= "-built";
	public const string XCreated 			= "-created"; //example use Score.AddScorePoints (unitName + DataScoreEnum.XCreated, 1);
	public const string XBuildLost		    = "-build lost"; //example of use Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
	public const string XUnitLost 	   		= "-unit lost";
	public const string XKilled			    = "-killed";
	public const string XDestroyed 			= "-destroyed";
}

public class Score : MonoBehaviour
{
	private Dictionary <string, Model.DataScore> dicDataScore;
	private List      <Model.DataScoreRanking> rankingScores;

	private Model.Player player;
	private Model.Battle battle;
	private DataScoreDAO dataScoreDAO;
	private PlayerBattleDAO playerBattleDAO; 
	
	public delegate void CallbackGetDataScore (Model.DataScore score);
	public delegate void OnLoadScoreCallback ();
	
	public delegate void GetScoresCallback (List <Model.DataScore> listScore);

	public delegate void OnLoadRanking (List <Model.DataScoreRanking> ranking);

	private bool wasInitialized = false;
	public bool NeedToSave = false;
	public bool isSaving = false;

	public void SaveScore ()
	{
		isSaving = true;
		dataScoreDAO.SaveScores
		(
			dicDataScore,
			(result) =>
			{
				if (result != null)
				{
					dicDataScore = result;
				}
				
				isSaving = false;
			}
		);
	}

	private Score Init ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();

		player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));
		
		if (pw.GetPropertyOnRoom ("battle") != null)
		{
			battle = new Model.Battle ((string)pw.GetPropertyOnRoom ("battle"));
		}

		dataScoreDAO = ComponentGetter.Get <DataScoreDAO> ();
		playerBattleDAO = ComponentGetter.Get <PlayerBattleDAO> ();

		return this;
	}

	private Model.DataScore _GetDataScore (string ScoreName, int battleId)
	{	

		string scoreKey = ScoreName + " - " + battleId;


		if (!dicDataScore.ContainsKey (scoreKey))
		{
			dicDataScore.Add (scoreKey, new Model.DataScore (player.IdPlayer, battleId, ScoreName, 0));
		}
		
		return dicDataScore[scoreKey];
	}
	
	private void _GetPlayerCurrentBattleScores (GetScoresCallback cb)
	{
		List<Model.DataScore> list = new List<Model.DataScore> ();
		
		foreach (Model.DataScore model in dicDataScore.Values)
		{
			if (model.IdPlayer == player.IdPlayer &&
			    model.IdBattle == battle.IdBattle)
			{
				list.Add (model);
			}		
		}
		
		cb (list);
	}

	private void _AddScorePoints (string ScoreName, int points, int battleId)
	{
		Model.DataScore score = _GetDataScore (ScoreName, battleId);
		score.NrPoints += points;
		NeedToSave = true;
	}

	private void _SubtractScorePoints (string ScoreName, int points, int battleId)
	{
		Model.DataScore score = _GetDataScore (ScoreName, battleId);
		score.NrPoints -= points;
		NeedToSave = true;
	}

	private void _SetScorePoints (string ScoreName, int points, int battleId)
	{
		Model.DataScore score = _GetDataScore (ScoreName, battleId);
		score.NrPoints = points;
		NeedToSave = true;
	}
	
	private void _LoadScore (OnLoadScoreCallback cb)
	{
		if (wasInitialized)
		{
			if (cb != null)
			{
				cb ();
			}
			return;
		}
		
		wasInitialized = true;
		
		//TODO ler apenas scores totais, ou seja, sem que tenham IdBattle's
		dataScoreDAO.LoadAllPlayerScores (player.ToDatabaseModel (),
											(scores) =>
											{
												dicDataScore = null;
												dicDataScore = scores;
												
												if (cb != null)
												{
													cb ();
												}
											});
	}

	private void _LoadBattleScore (GetScoresCallback cb)
	{
		if (ConfigurationData.battle != null)
		{
			dataScoreDAO.LoadAllBattleScores (ConfigurationData.battle.ToDatabaseModel (),
												(scores) =>
			                                  	{
													if (cb != null)
													{
														cb (scores);
													}
												});
		}	
		else
		{
			Debug.LogWarning ("ConfigurationData.battle is null!");
		}
	}
	
	private void _LoadRanking (OnLoadRanking cb)
	{
		playerBattleDAO.LoadBattleRankingScores
		(
			 (_rankingScores) =>
			 {
				rankingScores = _rankingScores;

				if (cb != null)
				{
					cb (rankingScores);
				}
			}
		);
	}
	
	/* Static */
	private static Score instance = null;
	private static Score Instance
	{
		get
		{
			if (instance == null)
			{
				instance = ComponentGetter.Get <Score> ("$$$_Score").Init ();
			}

			return instance;
		}
	}

	public static void AddScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._AddScorePoints (ScoreName, points, battleId);
	}

	public static void SubtractScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._SubtractScorePoints (ScoreName, points, battleId);
	}
	
	public static void GetDataScore (string scoreName, CallbackGetDataScore callback)
	{
		GetDataScore (scoreName, -1, callback);
	}
	
	public static void GetDataScore (string scoreName, int battleId, CallbackGetDataScore callback)
	{
		Model.DataScore score = Instance._GetDataScore (scoreName, battleId);
		callback (score);
	}
	
	public static void GetPlayerCurrentBattleScores (GetScoresCallback cb)
	{
		Instance._GetPlayerCurrentBattleScores (cb);
	}

	public static void SetScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._SetScorePoints (ScoreName, points, battleId);
	}

	public static void LoadScores (OnLoadScoreCallback cb = null)
	{
		Instance._LoadScore (cb);
	}
	
	public static void LoadBattleScore (GetScoresCallback cb = null)
	{
		Instance._LoadBattleScore (cb);
	}

	public static void LoadRanking (OnLoadRanking cb = null)
	{
		Instance._LoadRanking (cb);
	}
}