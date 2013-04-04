using UnityEngine;
using System.Collections;

using Visiorama;

public class NewGame : MonoBehaviour
{
	public UILabel messageActiveGame;

	float RefreshingInterval = 2.0f;
	bool wasInitialized      = false;

	PhotonWrapper pw;
	Transform buttons;

	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;
		messageActiveGame.enabled = false;

		pw = ComponentGetter.Get<PhotonWrapper> ();

		DefaultCallbackButton dcb;

		buttons = this.transform.FindChild ("Menu").FindChild ("Buttons");

		GameObject quickMatch = buttons.FindChild ("Quick Match").gameObject;

		dcb = quickMatch.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								pw.JoinQuickMatch ();
								//TODO fazer timeout de conexão
							});

		GameObject match1x1 = buttons.FindChild ("Match 1x1").gameObject;

		dcb = match1x1.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (2);
							});

		GameObject match2x2 = buttons.FindChild ("Match 2x2").gameObject;

		dcb = match2x2.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (3);
							});

		GameObject matchTxT = buttons.FindChild ("Match TxT").gameObject;

		dcb = matchTxT.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (4);
							});
	}

	public void Close ()
	{
		CancelInvoke ("TryToEnterGame");
	}

	private void CreateRoom (int maxPlayers)
	{
		string roomName = "Room" + PhotonNetwork.GetRoomList().Length + 1;
		bool isVisible = true, isOpen = true;

		pw.CreateRoom (roomName, isVisible, isOpen, maxPlayers);

		pw.SetPropertyOnPlayer ("team", 0);
		pw.SetPropertyOnPlayer ("ready", true);

		messageActiveGame.enabled = true;
		messageActiveGame.text = "Waiting For Other Players...";

		buttons.gameObject.SetActive (false);

		pw.TryToEnterGame (20.0f, (message) =>
									{
										Debug.Log("message: " + message);

										messageActiveGame.enabled = true;

										buttons.gameObject.SetActive (true);
									},
									(playersReady, nMaxPlayers) =>
									{
										messageActiveGame.text = "Wating For Other Players - "
																	+ playersReady + "/" + nMaxPlayers;

									});
	}
}
