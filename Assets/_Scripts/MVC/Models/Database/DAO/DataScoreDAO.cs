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
				Debug.Log ("lDataScore[i]: " + lScores[i]);
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
