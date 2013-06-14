using UnityEngine;
using System.Collections.Generic;

using Visiorama;

public class ShowScore : MonoBehaviour
{
	public class ScorePlayer
	{
		public int resourcesPoints;
		public int unitsPoints;
		public int structurePoints;
		
		public ScorePlayer ()
		{
			resourcesPoints = 0;
			unitsPoints = 0;
			structurePoints = 0;
		}
		
		public void AddScorePlayer (string scoreName, int points)
		{
			if (scoreName.Equals ("Resources gathered"))
			{
				resourcesPoints += points;
			}
			else if (scoreName.Equals ("Units created"))
			{
				unitsPoints += points;
			}
			else if (scoreName.Equals ("Units killed"))
			{
				unitsPoints += points;
			}
			else if (scoreName.Equals ("Units lost"))
			{
				unitsPoints -= points;
			}
			else if (scoreName.Equals ("Buildings created"))
			{
				structurePoints += points;
			}
			else if (scoreName.Equals ("Destroyed buildings"))
			{
				structurePoints += points;
			}
			else if (scoreName.Equals ("Buildings lost"))
			{
				structurePoints -= points;
			}
		}
		
		public string DebugPoints ()
		{
			return  "Resources: " + resourcesPoints.ToString () +
					"\n - Units: " + unitsPoints.ToString () +
					"\n - Structures: " + structurePoints.ToString ();
		}
	}
	
	public Transform scoreMenuObject;
	public GameObject scorePlayerPrefab;
	public Login login;
	public InternalMainMenu imm;
	
	public float startLabelPoisition;
	public float diferrenceBetweenLabels;
	
	// Use this for initialization
	void Awake ()
	{
		if (ConfigurationData.Logged &&
			ConfigurationData.InGame)
		{
			ConfigurationData.InGame = false;
			
			LoginIndex index = login.GetComponentInChildren<LoginIndex> ();
			index.SetActive (false);
			
			ActiveScoreMenu (true);
			
			Dictionary<int, ScorePlayer> players = new Dictionary<int, ScorePlayer>();
			
			Score.LoadBattle
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
						
						scorePlayerObject.transform.FindChild ("Name").GetComponentInChildren<UILabel> ().text = sp.Key.ToString ();
						scorePlayerObject.transform.FindChild ("Resources").GetComponentInChildren<UILabel> ().text = sp.Value.resourcesPoints.ToString ();
						scorePlayerObject.transform.FindChild ("Units").GetComponentInChildren<UILabel> ().text = sp.Value.unitsPoints.ToString ();
						scorePlayerObject.transform.FindChild ("Structures").GetComponentInChildren<UILabel> ().text = sp.Value.structurePoints.ToString ();
						
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
		else
		{
			ActiveScoreMenu (false);
		}
	}
	
	protected void ActiveScoreMenu (bool boolean)
	{
		scoreMenuObject.gameObject.SetActive (boolean);
	}
}
