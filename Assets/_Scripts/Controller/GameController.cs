using UnityEngine;
using System.Collections;
using Visiorama;

public class GameController : MonoBehaviour
{
	public bool autoload = false;

	public int targetFPS = 30;

	static bool wasInitialized = false;

	void Awake ()
	{
		Application.targetFrameRate = targetFPS;

		if (autoload && !wasInitialized)
		{
			wasInitialized = true;
			Application.LoadLevel(0);
			return;
		}

		ComponentGetter.Get<NetworkManager> ().Init ();
		ComponentGetter.Get<GameplayManager> ().Init ();
		ComponentGetter.Get<HUDController> ().Init ();
		ComponentGetter.Get<TouchController> ().Init ();
		ComponentGetter.Get<SelectionController> ().Init ();
		ComponentGetter.Get<StatsController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();
		ComponentGetter.Get<FogOfWar> ().Init ();
		ComponentGetter.Get<MiniMapController> ().Init ();
		ComponentGetter.Get<EventManager> ().Init ();
		
		
		Score.LoadScores (
			(dicScore) => 
			{
//				foreach (System.Collections.Generic.KeyValuePair<string, Model.DataScore> de in dicScore)
//				{
//					Debug.Log ("de.Key: " + de.Key + " - de.Value: " + de.Value);
//				}
				
				int manaAmount = dicScore.ContainsKey (DataScoreEnum.CurrentCrystals)
									? dicScore[DataScoreEnum.CurrentCrystals].NrPoints
									: 0;
				
				ResourcesManager resourcesManager = ComponentGetter.Get<GameplayManager> ().resources;

				resourcesManager.Mana = manaAmount;

				ComponentGetter.Get <BidManager> ().PayTheBid ();
			}
		);
		
	}
}
