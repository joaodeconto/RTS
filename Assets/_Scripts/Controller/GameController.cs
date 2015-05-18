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
		//Application.targetFrameRate = 60;

		if (autoload && !wasInitialized)
		{
			wasInitialized = true;
			Application.LoadLevel(1);
			return;
		}
		if(!PhotonNetwork.offlineMode)
		{
			ComponentGetter.Get<NetworkManager> ().Init ();
			Score.LoadScores (() => {ComponentGetter.Get <BidManager>().PayTheBid(); });
		}

		ComponentGetter.Get<GameplayManager> ().Init ();
		ComponentGetter.Get<StatsController> ().Init ();
		ComponentGetter.Get<SelectionController> ().Init ();
		ComponentGetter.Get<EventController> ().Init ();
		ComponentGetter.Get<TechTreeController> ().Init ();
		ComponentGetter.Get<HUDController> ().Init ();
		ComponentGetter.Get<FogOfWar> ().Init ();
		ComponentGetter.Get<MiniMapController> ().Init ();
		ComponentGetter.Get<TouchController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();

					
	}
}
