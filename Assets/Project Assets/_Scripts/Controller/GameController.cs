using UnityEngine;
using System.Collections;
using Visiorama;

public class GameController : MonoBehaviour
{
	void Awake ()
	{
		PhotonNetwork.offlineMode = true;

		ComponentGetter.Get<NetworkManager> ().Init ();
		ComponentGetter.Get<GameplayManager> ().Init ();
		ComponentGetter.Get<TouchController> ().Init ();
		ComponentGetter.Get<SelectionController> ().Init ();
		ComponentGetter.Get<TroopController> ().Init ();
		ComponentGetter.Get<FactoryController> ().Init ();
		ComponentGetter.Get<InteractionController> ().Init ();
	}
}
