using UnityEngine;
using System.Collections.Generic;

using Visiorama;

public class ShowScore : MonoBehaviour
{
	public class ScorePlayer
	{
		public int ResourcesPoints { get; private set; }
		public int UnitsPoints { get; private set; }
		public int StructurePoints { get; private set; }
		
		public int Total { get { return ResourcesPoints + UnitsPoints +	StructurePoints; } }
		
		public ScorePlayer ()
		{
			ResourcesPoints = 0;
			UnitsPoints 	= 0;
			StructurePoints = 0;
		}
		
		public void AddScorePlayer (string scoreName, int points)
		{
			if (scoreName.Equals (DataScoreEnum.ResourcesGathered))
			{
				ResourcesPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsCreated))
			{
				UnitsPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsKilled))
			{
				UnitsPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsLost))
			{
				UnitsPoints -= points;
			}
			else if (scoreName.Equals (DataScoreEnum.BuildingsCreated))
			{
				StructurePoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.DestroyedBuildings))
			{
				StructurePoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.BuildingsLost))
			{
				StructurePoints -= points;
			}
		}
		
		public string DebugPoints ()
		{
			return  "Resources: " + ResourcesPoints.ToString () +
					"\n - Units: " + UnitsPoints.ToString () +
					"\n - Structures: " + StructurePoints.ToString ();
		}
	}
	
	public Transform scoreMenuObject;
	public GameObject scorePlayerPrefab;
	public Login login;
	public InternalMainMenu imm;
	
	public float startLabelPoisition;
	public float diferrenceBetweenLabels;
	
	// Use this for initialization
	public void Init ()
	{
		LoginIndex index = login.GetComponentInChildren<LoginIndex> ();
		index.SetActive (false);
		
		ActiveScoreMenu (true);
		
		Dictionary<int, ScorePlayer> players = new Dictionary<int, ScorePlayer>();
		
		Score.LoadBattleScore
		(
			(dicScore) =>
			{
				for (int i = 0; i != dicScore.Count; i++)
				{
					if (!players.ContainsKey (dicScore[i].IdPlayer))
					{
						players.Add (dicScore[i].IdPlayer, new ScorePlayer ());
					}
					
					players[dicScore[i].IdPlayer].AddScorePlayer (dicScore[i].SzScoreName, dicScore[i].NrPoints);
				}
				
				float positionYInitial = startLabelPoisition;
				foreach (KeyValuePair<int, ScorePlayer> sp in players)
				{
					GameObject scorePlayerObject = NGUITools.AddChild (scoreMenuObject.gameObject, scorePlayerPrefab);
					scorePlayerObject.transform.localPosition = Vector3.up * positionYInitial;
					
					scorePlayerObject.transform.FindChild ("Name").GetComponentInChildren<UILabel> ().text 		 = sp.Key.ToString ();
					scorePlayerObject.transform.FindChild ("Resources").GetComponentInChildren<UILabel> ().text  = sp.Value.ResourcesPoints.ToString ();
					scorePlayerObject.transform.FindChild ("Units").GetComponentInChildren<UILabel> ().text 	 = sp.Value.UnitsPoints.ToString ();
					scorePlayerObject.transform.FindChild ("Structures").GetComponentInChildren<UILabel> ().text = sp.Value.StructurePoints.ToString ();
					scorePlayerObject.transform.FindChild ("Total").GetComponentInChildren<UILabel> ().text 	 = sp.Value.Total.ToString ();
					
					positionYInitial -= diferrenceBetweenLabels;
				}
			}
		);
		
		
		DefaultCallbackButton dcb = scoreMenuObject.FindChild ("Button Main Menu").gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null,
			(ht_dcb) =>
			{
				ActiveScoreMenu (false);
				imm.Init ();
			}
		);
	}
	
	protected void ActiveScoreMenu (bool boolean)
	{
		scoreMenuObject.gameObject.SetActive (boolean);
	}
}
