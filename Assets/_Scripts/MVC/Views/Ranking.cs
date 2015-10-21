using UnityEngine;
using System.Collections;
using System.Linq;
using Visiorama;
using System.Collections.Generic;

public class Ranking : MonoBehaviour {

	public GameObject rankRowPrefab;
	public Transform rankRowreference;
	public UIGrid rankGrid;
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
		if (wasInitialized)	return;
			
		wasInitialized = true;

//		rankLoading.SetActive(true);

		Score.LoadRanking 
			(
				(List<Model.DataScoreRanking> ranking) => 
				{
//					ranking.Sort((x, y) => {return y.NrPoints.CompareTo(x.NrPoints);});
					int i = 0;

					foreach (Model.DataScoreRanking r in ranking)
					{		
						i++;
						if ( i < 202)
						{	
						GameObject rankRow = NGUITools.AddChild (rankGrid.gameObject, rankRowPrefab);
						rankRowPrefab.GetComponent<RankRow>().player.text = r.SzName;
						rankRowPrefab.GetComponent<RankRow>().wins.text = r.NrVictory.ToString();
						rankRowPrefab.GetComponent<RankRow>().defeats.text = r.NrDefeat.ToString();
						rankRowPrefab.GetComponent<RankRow>().score.text = r.NrPoints.ToString();
						rankRowPrefab.GetComponent<RankRow>().position.text = i.ToString();
						rankRowPrefab.name = i.ToString();
						}
						else 
						{
//							rankLoading.SetActive(false);
							rankGrid.repositionNow = true;
							break;
						}
					}

				NGUITools.Destroy(rankGrid.transform.GetChild(0).gameObject);
				rankGrid.repositionNow = true;

			}
			);	
	}
		
	public void Close ()
	{
//		rankLoading.SetActive(false);
    }
    
	
}

