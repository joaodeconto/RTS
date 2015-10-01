using UnityEngine;
using System.Collections;
using Visiorama;

public class ActiveGames : MonoBehaviour
{
	public float RefreshingInterval = 1.0f;
	public UILabel messageActiveGame;
	public UILabel errorMessage;
	public GameObject pref_Row;
	public Transform rowsContainer;

	public void Open ()
	{
		messageActiveGame.enabled = false;
		GameObject btn = transform.FindChild ("Menu").FindChild ("Button Leave Room").gameObject;
		DefaultCallbackButton dcb = btn.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null, (ht) =>{
			if (ComponentGetter.Get<PhotonWrapper> ().LeaveRoom ())	Close ();
		});
		btn.SetActive (false);
		btn = transform.FindChild("Menu").FindChild("Button (Facebook)").gameObject;
		dcb = btn.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null, (ht) =>
		          {
			FB.AppRequest( message:"Join me in a free Multiplayer RTS - Rex Tribal Society!", title:"3D Real-Time Strategy for mobile!");

		});
		InvokeRepeating ("Refresh", 0.0f, RefreshingInterval);
	}

	public void Close ()
	{
		CancelInvoke ("Refresh");
		ClearRows ();
		messageActiveGame.enabled = errorMessage.enabled = false;
		GameObject leaveRoom = transform.FindChild ("Menu").FindChild ("Button Leave Room").gameObject;
		leaveRoom.SetActive (false);
		if (PhotonNetwork.connectionState == ConnectionState.Connected)	PhotonNetwork.Disconnect();
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
		if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
		{
			PhotonNetwork.ConnectToMaster ( PhotonNetwork.PhotonServerSettings.ServerAddress,
			                               PhotonNetwork.PhotonServerSettings.ServerPort,
			                               PhotonNetwork.PhotonServerSettings.AppID,
			                               ConfigurationData.VERSION);
		}

		ClearRows ();
		PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
		RoomInfo[] roomQuery = pw.GetRoomList();
		int counter = 0;
		if (roomQuery.Length == 0)	{}
		else
		{
			foreach (RoomInfo room in roomQuery)
			{
				bool isRoomClosed = (bool)room.customProperties["closeRoom"];
				if (!isRoomClosed) isRoomClosed = (room.playerCount == room.maxPlayers);

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

				//TODO Adicionar Label de Bid, conferir o prefab. 

				GameObject join = trns.FindChild ("Join").gameObject;

				if (isRoomClosed){
					join.SetActive (false);
				}
				else{
					join.SetActive (true);
					Hashtable ht = new Hashtable ();
					ht["room.name"] = room.name;

					if (join.GetComponent<DefaultCallbackButton> () == null){
						DefaultCallbackButton dcb = join.AddComponent<DefaultCallbackButton> ();
						dcb.Init (ht, (ht_hud) =>{
							pw.JoinRoom ((string)ht_hud["room.name"]);
							pw.SetPropertyOnPlayer ("ready", true);							
							ClearRows ();
							messageActiveGame.enabled = true;
							messageActiveGame.text = "Joined Battle";
							GameObject btnFace = transform.FindChild("Menu").FindChild("Button (Facebook)").gameObject;
							GameObject cancel = transform.FindChild ("Menu").FindChild ("Button Leave Room").gameObject;
							cancel.SetActive (true);
							btnFace.SetActive(false);
							cancel.AddComponent<DefaultCallbackButton>().Init (null,(_ht) =>{
								pw.LeaveRoom ();
								btnFace.SetActive(true);
								InvokeRepeating ("Refresh", 0.0f, RefreshingInterval);
							});
							CancelInvoke ("Refresh");
							pw.TryToEnterGame ( 100000.0f,
							                   (message) =>{
													//Debug.Log("message: " + message);
													messageActiveGame.enabled = true;
													errorMessage.enabled = false;
													Invoke ("CloseErrorMessage", 5.0f);
													InvokeRepeating ("Refresh", 0.0f, RefreshingInterval);
												},
												(playersReady, maxPlayers) =>{
													messageActiveGame.text = "Waiting Players - "+playersReady+"/"+maxPlayers;

												});
						});
					}
				}
			}
		}
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
