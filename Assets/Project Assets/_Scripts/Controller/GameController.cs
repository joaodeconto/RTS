using UnityEngine;
using System.Collections;
using Visiorama;

public class GameController : MonoBehaviour
{
	public bool autoload = false;

	static bool wasInitialized = false;

	void Awake ()
	{
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
		ComponentGetter.Get<TroopController> ().Init ();
		ComponentGetter.Get<FactoryController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();
		ComponentGetter.Get<FogOfWar> ().Init ();
		ComponentGetter.Get<MiniMapController> ().Init ();
		ComponentGetter.Get<EventManager> ().Init ();
		ComponentGetter.Get<MessageInfoManager> ().Init ();
	}
}
