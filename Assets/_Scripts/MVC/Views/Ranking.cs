using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Ranking : MonoBehaviour {

	public GameObject rankRowPrefab;
	public Transform rankRowreference;
	public UIGrid rankGrid;
	private int i = 0;

	public bool wasInitialized = false;

	public void OnEnable ()
	{
		Open ();
	}
	
	public void OnDisable ()
	{
		Close ();
	}

	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;

		Score.LoadRanking 
			(
				(System.Collections.Generic.List<Model.DataScoreRanking> ranking) => 
				{
			
				
				ranking.Sort((x, y) => {
					return y.NrPoints.CompareTo(x.NrPoints);
				});
				
				foreach (Model.DataScoreRanking r in ranking)
				{

					i++;
					GameObject rankRow = NGUITools.AddChild (rankGrid.gameObject, rankRowPrefab);
					rankRowPrefab.GetComponent<RankRow>().player.text = r.SzName;
					rankRowPrefab.GetComponent<RankRow>().wins.text = r.NrVictory.ToString();
					rankRowPrefab.GetComponent<RankRow>().defeats.text = r.NrDefeat.ToString();
					rankRowPrefab.GetComponent<RankRow>().score.text = r.NrPoints.ToString();
					rankRowPrefab.GetComponent<RankRow>().position.text = i.ToString();


				}

			}
			);
		rankGrid.repositionNow = true;


	}
	public void Close ()
	{

    }
    
	
}

