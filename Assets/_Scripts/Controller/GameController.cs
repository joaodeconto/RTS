using UnityEngine;
using System.Collections;
using Visiorama;

public class GameController : MonoBehaviour
{
	public bool autoload = false;

	public int targetFPS = 60;

	static bool wasInitialized = false;

	void Awake ()
	{
		Application.targetFrameRate = 60;

		if (autoload && !wasInitialized)
		{
			wasInitialized = true;
			Application.LoadLevel(0);
			return;
		}

		ComponentGetter.Get<NetworkManager> ().Init ();
		ComponentGetter.Get<GameplayManager> ().Init ();
		ComponentGetter.Get<TouchController> ().Init ();
		ComponentGetter.Get<SelectionController> ().Init ();
		ComponentGetter.Get<StatsController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();
		ComponentGetter.Get<EventController> ().Init ();
		ComponentGetter.Get<TechTreeController> ().Init ();
		ComponentGetter.Get<HUDController> ().Init ();
		ComponentGetter.Get<FogOfWar> ().Init ();
		ComponentGetter.Get<MiniMapController> ().Init ();

		Score.LoadScores (
			() => 
			{
//				foreach (System.Collections.Generic.KeyValuePair<string, Model.DataScore> de in dicScore)
//				{
//					Debug.Log ("de.Key: " + de.Key + " - de.Value: " + de.Value);
//				}

				ComponentGetter.Get <BidManager> ().PayTheBid ();
		
				Score.GetDataScore
				(
					DataScoreEnum.CurrentCrystals,
					(currentCrystals) =>
					{
						ComponentGetter.Get<GameplayManager> ().resources.Mana = currentCrystals.NrPoints;
					}
				);
			}
		);
		
	}

	public void GameStartInit()
	{
	}
}
