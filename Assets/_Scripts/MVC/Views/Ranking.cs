using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ranking : MonoBehaviour {
	
	// Use this for initialization 
	void OnEnable ()
	{
		Score.LoadRanking 
			(
				(System.Collections.Generic.List<Model.DataScoreRanking> ranking) => 
				{
				Debug.Log ("Chegou ranking\t");
				//Separar por nome do datascore victory ou defeat
				foreach (Model.DataScoreRanking r in ranking)
				{
					Debug.Log ("Player: " + r.SzName + " : " + r.NrPoints + "\n");					
				}
			}
			);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
