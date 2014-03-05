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
		List <DB.DataScore> lDataScore = new List <DB.DataScore> ();
		db.Read (player, lDataScore.GetType (), "read-list-datascore",
		(response) =>
		{
			Dictionary <string, Model.DataScore> dic = new Dictionary <string, Model.DataScore> ();

			if ((response as List <DB.DataScore>) != null)
			{
				lDataScore = (response as List <DB.DataScore>);
			}

			for (int i = lDataScore.Count - 1; i != -1; --i)
			{
				Debug.Log ("lDataScore[i]: " + lDataScore[i]);
				dic.Add (lDataScore[i].SzScoreName + " - " + lDataScore[i].IdBattle, lDataScore[i].ToModel ());
			}
			
			callback (dic);
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

	public void SaveScores (Dictionary <string, Model.DataScore> scores)
	{
		List<Model.DataScore> lScores = new List<Model.DataScore> ();

		foreach (KeyValuePair <string, Model.DataScore> de in scores)
		{
			lScores.Add (de.Value);
		}

		db.Update (lScores, "save-list-datascore",
		(response) =>
		{
			Debug.Log ("response save-list-datascore: " + response);
		});
	}
}
