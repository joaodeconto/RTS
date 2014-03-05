using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using Visiorama;

public struct DataScoreEnum
{
	public const string CurrentCrystals = "current-crystals";
	public const string TotalCrystals = "total-crystals";
	
	public const string UnitsCreated = "units-created";
	public const string BuildingsCreated = "units-created";
	public const string XCreated = "-created"; //example use Score.AddScorePoints (unitName + DataScoreEnum.XCreated, 1);
	
	public const string BuildingsLost = "buildings-lost";
	public const string XLost = "-lost"; //example of use Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
	
	public const string DestroyedBuildings = "destroyed-buildings";
	public const string XDestroyed = " destroyed";
}

public class Score : MonoBehaviour
{
	private Dictionary <string, Model.DataScore> dicDataScore;
	private List <Model.DataScore> dicCurrentBattleScore;
	private Model.Player player;
	private DataScoreDAO dataScoreDAO;
	
	public delegate void GetScoresCallback (List <Model.DataScore> listScore);
	public delegate void GetDicScoresCallback (Dictionary <string, Model.DataScore> dicScore);
	public GetScoresCallback getScoresCallback;

	private Score Init ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));

		dataScoreDAO = ComponentGetter.Get <DataScoreDAO> ();
		dicDataScore = new Dictionary <string, Model.DataScore> ();
		dicCurrentBattleScore = new List<Model.DataScore> ();

		return this;
	}

	void CreateDataScore (string ScoreName, int points, int battleId)
	{
		Model.DataScore ds = new Model.DataScore (player.IdPlayer, ScoreName, points);

		if (battleId != -1) ds.IdBattle = battleId;

		string scoreKey = GetScoreKey (ScoreName, battleId);

		dicDataScore.Add (scoreKey, ds);
	}

	private string GetScoreKey (string ScoreName, int battleId)
	{
		return ScoreName + " - " + battleId;
	}

	public void _AddScorePoints (string ScoreName, int points = 1, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints += points;
	}

	public void _SubtractScorePoints (string ScoreName, int points, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints -= points;
	}

	public void _SetScorePoints (string ScoreName, int points, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints = points;
	}
	
	private void _LoadScore (GetDicScoresCallback cb)
	{
		//TODO ler apenas scores totais, ou seja, sem que tenham IdBattle's
		dataScoreDAO.LoadAllPlayerScores (player.ToDatabaseModel (),
											(scores) =>
											{
												dicDataScore = scores;
												
												cb (dicDataScore);
											});
	}
	
	private Model.DataScore _GetDataScore (string scoreName, int idBattle)
	{
		string key = scoreName + " - " + idBattle;
		
		Model.DataScore ds = null;
		
		if (dicDataScore != null)
		{
			if (dicDataScore.ContainsKey (key))
				ds = dicDataScore[key];
		}
		
		return ds;
	}
	
	private void _LoadBattleScore (GetScoresCallback b)
	{
		if (ConfigurationData.battle != null)
		{
			dataScoreDAO.LoadAllBattleScores (ConfigurationData.battle.ToDatabaseModel (),
												(scores) =>
												{
													dicCurrentBattleScore = scores;
				
													b (dicCurrentBattleScore);
												});
		}	
		else
		{
			Debug.LogWarning ("ConfigurationData.battle is null!");
		}
	}

	public void _SaveScore ()
	{
		dataScoreDAO.SaveScores (dicDataScore);
	}
	
	public List<Model.DataScore> CurrentBattle ()
	{
		return dicCurrentBattleScore;
	}

	/* Static */
	private static Score instance;
	public static Score Instance
	{
		get
		{
			if (instance == null)
			{
				instance = ComponentGetter.Get <Score> ().Init ();
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

	public static void SetScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._SetScorePoints (ScoreName, points, battleId);
	}

	public static void LoadScores (GetDicScoresCallback cb)
	{
		Instance._LoadScore (cb);
	}

	public static Model.DataScore GetDataScore (string scoreName, int idBattle = 0)
	{
		return Instance._GetDataScore (scoreName, idBattle);
	}
	
	public static void LoadBattle (GetScoresCallback cb)
	{
		Instance._LoadBattleScore (cb);
	}

	public static void Save ()
	{
		Instance._SaveScore ();
	}
}