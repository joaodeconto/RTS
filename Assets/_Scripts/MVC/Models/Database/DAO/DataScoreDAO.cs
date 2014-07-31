using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Newtonsoft.Json;

public class DataScoreDAO : MonoBehaviour
{
	private Database db;

	void Awake () { db = ComponentGetter.Get <Database> (); }

	public delegate void DataScoreDAODelegateDictionary (Dictionary <string, Model.DataScore> scores);
	public void LoadAllPlayerScores (DB.Player player, DataScoreDAODelegateDictionary callback)
	{
		List <DB.DataScore> lScores = new List <DB.DataScore> ();
		db.Read (player, lScores.GetType (), "read-list-datascore",
		(response) =>
		{
			if ((response as List <DB.DataScore>) != null)
			{
				lScores = (response as List <DB.DataScore>);
			}

			Dictionary <string, Model.DataScore> scores = new Dictionary <string, Model.DataScore> ();
			
			for (int i = lScores.Count - 1; i != -1; --i)
			{
//				if (lDataScore[i].SzScoreName.Contains ("current"))
//				Debug.Log ("lDataScore[i]: " + lScores[i]);
				string key = lScores[i].SzScoreName + " - " + lScores[i].IdBattle;
				
				if (!scores.ContainsKey (key))
					scores.Add (key, lScores[i].ToModel ());
				else
					scores[key] = lScores[i].ToModel ();
			}
			
			callback (scores);
		});
	}
	
	public delegate void DataScoreDAODelegateList (List<Model.DataScore> scores);
	public void LoadAllBattleScores (DB.Battle battle, DataScoreDAODelegateList callback)
	{
		List <DB.DataScore> lDataScore = new List <DB.DataScore> ();
		db.Read (battle, lDataScore.GetType (), "read-battle-datascore",
		(response) =>
		{
			List<Model.DataScore> list = new List<Model.DataScore> ();

			if ((response as List <DB.DataScore>) != null)
			{
				lDataScore = (response as List <DB.DataScore>);
			}

			for (int i = lDataScore.Count - 1; i != -1; --i)
			{
				list.Add (lDataScore[i].ToModel ());
			}

			callback (list);
		});
	}
	public delegate void RankingDAODelegate ( List<Model.DataScoreRanking> ranking);
	public void LoadRankingScores (RankingDAODelegate callback)
	{
		//Cria objetos que serao usados durante o chamado ao DB
		object dummyObject = new object ();
		
		List <Model.DataScoreRanking> lDataScoreRanking = new List <Model.DataScoreRanking> ();
		
		//Chama todos os datascores de vitoria e derrotas
		db.Read (dummyObject, lDataScoreRanking.GetType (), "read-datascore-ranking",
		         (response) =>
		{
			if ((response as List <Model.DataScoreRanking>) != null)
				{
				lDataScoreRanking = (response as List <Model.DataScoreRanking>);		
					Debug.Log ("lDataScoreRanking.Count: " + lDataScoreRanking.Count);
				}			
				//Debug.Break ();
				callback (lDataScoreRanking);			
		});
	}
	public void LoadRankingScoresOld (RankingDAODelegate callback)
	{
		//Cria objetos que serao usados durante o chamado ao DB
		object dummyObject = new object ();
		
		List <DB.DataScore> lDataScore = new List <DB.DataScore> ();
		List <DB.Player> lPlayers = new List<DB.Player> ();
		
		Model.DataScore cDataScore = null;
		Model.Player cPlayer = null;
		
		List <Model.DataScore> tmpList = new List<Model.DataScore> ();
		
		//Chama todos os datascores de vitoria e derrotas
		db.Read (dummyObject, lDataScore.GetType (), "read-datascore-ranking",
		         (response) =>
		         {
			if ((response as List <DB.DataScore>) != null)
			{
				lDataScore = (response as List <DB.DataScore>);
				
				//As linhas comentadas abaixo servem para debug
				//foreach (DB.DataScore ds in lDataScore)
				//{
				//	Debug.Log (ds.SzScoreName + " - " + ds.NrPoints);
				//}
				
				//Debug.Log ("lDataScore.Count: " + lDataScore.Count);
			}
			
			//Debug.Break ();
			
			//Muda de lScore para sScore para melhorar performance
			Stack <Model.DataScore> sDataScore = new Stack<Model.DataScore> ();
			
			for (int i = 0, imax = lDataScore.Count; i != imax; ++i)
			{
				sDataScore.Push(lDataScore[i].ToModel ());
			}
			
			dummyObject = new object ();
			
			//Chama todos os players
			db.Read (dummyObject, lPlayers.GetType (), "read-all-players",
			         (_response) =>
			         {
				if ((_response as List <DB.Player>) != null)
				{
					lPlayers = (_response as List <DB.Player>);
					Debug.Log ("lPlayers.Count: " + lPlayers.Count);
				}
				else
				{
					//Debug.Log ("_response: " + _response.GetType ());
				}
				
				Debug.Log ("Chegou 2");
				
				//Ranking com os data scores por player, que sera enviado a quem chamou a funcao
				Dictionary <Model.Player, List<Model.DataScore>> scores = new Dictionary<Model.Player, List<Model.DataScore>> ();
				
				for (int p = 0, pmax = lPlayers.Count; p != pmax; ++pmax)
				{
					cPlayer = lPlayers[p].ToModel ();
					
					while (sDataScore.Count != 0)
					{
						//					Debug.Log ("Chegou 3");
						cDataScore = sDataScore.Pop ();
						
						if (cDataScore.IdPlayer.ToString () == lPlayers[p].IdPlayer)
						{
							//							Debug.Log ("Chegou 4");
							if (!scores.ContainsKey (cPlayer))
							{
								//								Debug.Log ("Chegou 5");
								tmpList = new List<Model.DataScore> ();
								tmpList.Add (cDataScore);
								scores.Add (cPlayer, tmpList);
							}
							else
							{
								//								Debug.Log ("Chegou 6");
								scores[cPlayer].Add (cDataScore);
							}
						}
					}
				}
				
				Debug.Log ("Chegou 7");
				//				callback (scores);
			});
		});
	}
	
	public void SaveScores (Dictionary <string, Model.DataScore> scores, DataScoreDAODelegateDictionary callback)
	{
		List<Model.DataScore> lScores = new List<Model.DataScore> ();

		Debug.Log ("Antes!");
		foreach (KeyValuePair <string, Model.DataScore> de in scores)
		{
			Debug.Log ("stringa: " + de.Key + " - IdDataScore: " + de.Value.IdDataScore + " - SzScoreName: " + de.Value.SzScoreName + " - NrPoints: "  + de.Value.NrPoints);
			lScores.Add (de.Value);
		}

		db.Update (lScores, "save-list-datascore",
		(response) =>
		{
			lScores = (List<Model.DataScore>)response;
			scores  = new Dictionary<string, Model.DataScore> ();
//			/*
//			Debug.Log ("response save-list-datascore: " + response);
			Debug.Log ("Depois!");
			
			if (lScores != null)
			{
				for (int i = lScores.Count - 1; i != -1; --i)
				{
//					if (lScores[i].SzScoreName.Contains ("current"))
//						Debug.Log ("IdDataScore: " + lScores[i].IdDataScore + " - SzScoreName: " + lScores[i].SzScoreName + " - NrPoints: "  + lScores[i].NrPoints);
					
					string key = lScores[i].SzScoreName + " - " + lScores[i].IdBattle;
					if (!scores.ContainsKey (key))
						scores.Add (key, lScores[i]);
					else
						scores[key] = lScores[i];
				}
			}
			
			foreach (KeyValuePair <string, Model.DataScore> de in scores)
			{
				Debug.Log ("stringa: " + de.Key + " - IdDataScore: " + de.Value.IdDataScore + " - SzScoreName: " + de.Value.SzScoreName + " - NrPoints: "  + de.Value.NrPoints);
				lScores.Add (de.Value);
			}
//			*/
			callback (scores);
		});
	}
}
