using UnityEngine;

using System;
using System.Collections;

using Visiorama;

using Hashtable = ExitGames.Client.Photon.Hashtable;

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
		Debug.Log ("player na segunda cena: " + ComponentGetter.Get<PhotonWrapper> ().GetPropertyOnPlayer ("player"));
//		if (wasInitialized)
//			return;

//		wasInitialized = true;
		messageActiveGame.enabled = false;

		pw = ComponentGetter.Get<PhotonWrapper> ();

		DefaultCallbackButton dcb;

		buttons = this.transform.FindChild ("Menu").FindChild ("Buttons");

		GameObject tutorial = buttons.FindChild ("Tutorial").gameObject;

		dcb = tutorial.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								CreateRoom (1, "1P");
							});

//		GameObject match3p = buttons.FindChild ("Match 3P").gameObject;
//
//		dcb = match3p.AddComponent<DefaultCallbackButton> ();
//		dcb.Init(null, (ht_hud) =>
//							{
//								CreateRoom (3, "3P");
//							});
//
//		GameObject match4p = buttons.FindChild ("Match 4P").gameObject;
//
//		dcb = match4p.AddComponent<DefaultCallbackButton> ();
//		dcb.Init(null, (ht_hud) =>
//							{
//								CreateRoom (4, "4P");
//							});
//
//		GameObject match2x2 = buttons.FindChild ("Match 2x2").gameObject;
//
//		dcb = match2x2.AddComponent<DefaultCallbackButton> ();
//		dcb.Init(null, (ht_hud) =>
//							{
//								CreateRoom (4, "2x2", GameplayManager.Mode.Cooperative);
//							});

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

	private void CreateRoom (int maxPlayers, string battleTypeName, GameplayManager.Mode mode = GameplayManager.Mode.Deathmatch)
	{
		Model.Player player = ComponentGetter.Get <InternalMainMenu>().player;
		PlayerBattleDAO playerBattleDao = ComponentGetter.Get <PlayerBattleDAO> ();
		PlayerDAO playerDao = ComponentGetter.Get <PlayerDAO> ();

		playerBattleDao.CreateBattle (battleTypeName, DateTime.Now, maxPlayers,
		(battle) =>
		{
			//Debug.Log ("message: " + message);
			//Debug.Log ("playerBattle: " + playerBattle);

			string roomName = "Room" + (PhotonNetwork.GetRoomList().Length + 1) + " : " + System.DateTime.Now.ToString ("mm-ss");
			bool isVisible = true, isOpen = true;
			
			ConfigurationData.battle = battle;
			
			Hashtable properties = new Hashtable ();
			properties.Add ("battle", battle.ToString ());
			pw.CreateRoom (roomName, isVisible, isOpen, maxPlayers, properties);

			pw.SetPropertyOnPlayer ("team", 0);
			pw.SetPropertyOnPlayer ("ready", true);

			VDebug.Log ("battle: " + properties["battle"]);

			GameplayManager.mode = mode;

			messageActiveGame.enabled = true;
			messageActiveGame.text = "Waiting For Other Players...";

			foreach (Transform button in buttons)
			{
				button.gameObject.SetActive (false);
			}

			GameObject leaveRoom = buttons.FindChild ("Leave Room").gameObject;
			leaveRoom.SetActive (true);

			pw.TryToEnterGame (10000.0f,
			(other_message) =>
			{
				//Debug.Log("message: " + message);

				//messageActiveGame.enabled = false;

				//buttons.gameObject.SetActive (true);

				//errorMessage.enabled = true;

				//Invoke ("CloseErrorMessage", 5.0f);
			},
			(playersReady, nMaxPlayers) =>
			{
				messageActiveGame.text = "Waiting For Other Players - " + playersReady + "/" + nMaxPlayers;
			});
		});
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
