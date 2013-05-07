using UnityEngine;
using System.Collections;

using Visiorama;

public class NewGame : MonoBehaviour
{
	public UILabel messageActiveGame;
	public UILabel errorMessage;

	float RefreshingInterval = 2.0f;
	bool wasInitialized      = false;

	PhotonWrapper pw;
	Transform buttons;

	public void Open ()
	{
//		if (wasInitialized)
//			return;
//
//		wasInitialized = true;
		messageActiveGame.enabled = false;

		pw = ComponentGetter.Get<PhotonWrapper> ();

		DefaultCallbackButton dcb;

		buttons = this.transform.FindChild ("Menu").FindChild ("Buttons");

		GameObject match2p = buttons.FindChild ("Match 2P").gameObject;

		dcb = match2p.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (2);
							});

		GameObject match3p = buttons.FindChild ("Match 3P").gameObject;

		dcb = match3p.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (3);
							});

		GameObject match4p = buttons.FindChild ("Match 4P").gameObject;

		dcb = match4p.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (4);
							});
		
		GameObject match2x2 = buttons.FindChild ("Match 2x2").gameObject;

		dcb = match2x2.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (4, GameplayManager.Mode.Allies);
							});
		
		GameObject leaveRoom = buttons.FindChild ("Leave Room").gameObject;
		
		dcb = leaveRoom.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null, (ht) => 
							{
								if (pw.LeaveRoom ())
									Close ();
							});
		
		leaveRoom.SetActive (false);
	}

	public void Close ()
	{
		CancelInvoke ("TryToEnterGame");
		
		if (buttons == null) return;
		
		foreach (Transform button in buttons)
		{
			button.gameObject.SetActive (true);
		}

		messageActiveGame.enabled = false;
		
		GameObject leaveRoom = buttons.FindChild ("Leave Room").gameObject;
		leaveRoom.SetActive (false);
	}

	private void CreateRoom (int maxPlayers, GameplayManager.Mode mode = GameplayManager.Mode.Normal)
	{
		GameplayManager.mode = mode;
		
		string roomName = "Room" + (PhotonNetwork.GetRoomList().Length + 1) + " : " + System.DateTime.Now.ToString ("mm-ss");
		bool isVisible = true, isOpen = true;

		pw.CreateRoom (roomName, isVisible, isOpen, maxPlayers);

		pw.SetPropertyOnPlayer ("team", 0);
		pw.SetPropertyOnPlayer ("ready", true);

		messageActiveGame.enabled = true;
		messageActiveGame.text = "Waiting For Other Players...";
		
		foreach (Transform button in buttons)
		{
			button.gameObject.SetActive (false);
		}

		GameObject leaveRoom = buttons.FindChild ("Leave Room").gameObject;
		leaveRoom.SetActive (true);
		
		pw.TryToEnterGame (10000.0f, (message) =>
									{
//										Debug.Log("message: " + message);
//
//										messageActiveGame.enabled = false;
//
//										buttons.gameObject.SetActive (true);
//
//										errorMessage.enabled = true;
//
//										Invoke ("CloseErrorMessage", 5.0f);
									},
									(playersReady, nMaxPlayers) =>
									{
										messageActiveGame.text = "Wating For Other Players - "
																	+ playersReady + "/" + nMaxPlayers;
									});
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
