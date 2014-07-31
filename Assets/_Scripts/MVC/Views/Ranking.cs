using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ranking : MonoBehaviour {


	public bool wasInitialized = false;
	
	public void OnEnable ()
	{
		Open ();
	}
	
	public void OnDisable ()
    {
        Close ();
    }
	void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;
		
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
	
	public void Close ()
	{
		gameObject.SetActive (false);
    }
    
	
}
