using UnityEngine;

using System.Collections;
using System.Linq;

public class ActiveGames : MonoBehaviour
{
	public float RefreshingInterval = 2.0f;
	public UILabel messageActiveGame;
	public GameObject pref_Row;
	public Transform rowsContainer;

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
		messageActiveGame.enabled = false;

		InvokeRepeating ("Refresh", 0.0f, RefreshingInterval);
	}

	public void Close ()
	{
		CancelInvoke ("Refresh");
		ClearRows ();
	}

	private void ClearRows ()
	{
		foreach (Transform t in rowsContainer)
		{
			Destroy (t.gameObject);
		}
	}

	private void Refresh ()
	{
		ClearRows ();

		RoomInfo[] roomQuery = (from r in PhotonNetwork.GetRoomList ()
									where ((bool)r.customProperties["closeRoom"] == false)
									   && (r.playerCount != r.maxPlayers)
									select r).ToArray ();

		int counter = 0;
		if (roomQuery.Length == 0)
			;
			//GUILayout.Label ("There's anywhere room");
		else
			foreach (RoomInfo room in roomQuery)
			{
				bool isRoomClosed = (bool)room.customProperties["closeRoom"];

				GameObject r = Instantiate (pref_Row, Vector3.zero, Quaternion.identity) as GameObject;

				r.name = "Row" + (++counter);

				Transform trns = r.transform;

				trns.parent = rowsContainer;
				trns.localScale       = Vector3.one;
				trns.localPosition    = Vector3.up * (-32 * counter);
				trns.localEulerAngles = Vector3.zero;

				trns.FindChild ("Name").GetComponent<UILabel>().text    = room.name;
				trns.FindChild ("Players").GetComponent<UILabel>().text = room.playerCount + "/" + room.maxPlayers;
				trns.FindChild ("Status").GetComponent<UILabel>().text  = isRoomClosed ? "Closed" : "Open";

				GameObject join = trns.FindChild ("Join").gameObject;

				if (isRoomClosed)
				{
					join.SetActive (false);
				}
				else
				{
					join.SetActive (true);

					Hashtable ht = new Hashtable ();

					ht["room.name"] = room.name;

					DefaultCallbackButton dcb = join.AddComponent<DefaultCallbackButton> ();
					dcb.Init (ht, (ht_hud) =>
										{
											PhotonNetwork.JoinRoom ((string)ht_hud["room.name"]);
											PhotonNetwork.player.customProperties["ready"] = true;
											PhotonNetwork.player.customProperties["team"]  = PhotonNetwork.playerList.Length-1;


											ClearRows ();

											messageActiveGame.enabled = true;
											messageActiveGame.text = "Wating For Other Players";

											CancelInvoke ("Refresh");
											InvokeRepeating ("TryToEnterGame", 0.0f, RefreshingInterval);
										});
				}
			}
	}

	private void TryToEnterGame ()
	{
		if (PhotonNetwork.room == null)
		{
			Debug.Log("Algo estranho por aqui");
			return;
		}

		int numberOfReady = 0;
		foreach (PhotonPlayer p in PhotonNetwork.playerList)
		{
			if (      p.customProperties.ContainsKey("ready") &&
			    (bool)p.customProperties["ready"] == true)
			{
				numberOfReady++;
			}
		}

		if (numberOfReady == PhotonNetwork.room.maxPlayers)
		{
			if (PhotonNetwork.isMasterClient)
			{
				Hashtable roomProperty = new Hashtable() {{"closeRoom", true}};
				PhotonNetwork.room.SetCustomProperties(roomProperty);
			}
			StartCoroutine (StartGame ());
		}

		messageActiveGame.text = "Wating For Other Players - " + numberOfReady + "/" + PhotonNetwork.room.maxPlayers;
	}

	private IEnumerator StartGame ()
    {
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }

        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel(1);
    }
}
