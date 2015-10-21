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
		if (autoload && !wasInitialized){
			wasInitialized = true;
			Application.LoadLevel(1);
			return;
		}
		if(!PhotonNetwork.offlineMode){
			ComponentGetter.Get<NetworkManager> ().Init ();
			Score.LoadScores (() => {ComponentGetter.Get <BidManager>().PayTheBid(); });
		}
		else{
			GameObject oScore = new GameObject("OfflineScore"); 
			oScore.AddComponent("OfflineScore");
		}

		ComponentGetter.Get<GameplayManager> ().Init ();
		ComponentGetter.Get<TouchController> ().Init ();
		ComponentGetter.Get<SelectionController> ().Init ();
		ComponentGetter.Get<StatsController> ().Init ();
		ComponentGetter.Get<EventController> ().Init ();
		ComponentGetter.Get<TechTreeController> ().Init ();
		ComponentGetter.Get<HUDController> ().Init ();
		ComponentGetter.Get<FogOfWar> ().Init ();
		ComponentGetter.Get<MiniMapController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();
					
//		if(Everyplay.IsRecordingSupported())
//		{
//			Everyplay.StartRecording();
//		}
	}
}
