using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour
{
	public UIInput username;
	public UIInput password;
	public GameObject submitButton;
	public GameObject mainMenu;

	public NetworkGUI networkGUI;

	public void Start ()
	{
		Init ();
	}

	private bool wasInitialized = false;
	public void Init ()
	{
		if (wasInitialized) return;

		wasInitialized = true;

		Application.runInBackground = true;

		if (!PhotonNetwork.connected)
			 PhotonNetwork.ConnectUsingSettings (ConfigurationData.VERSION);

		DefaultCallbackButton dcb = submitButton.AddComponent<DefaultCallbackButton>();

		Hashtable ht = new Hashtable();
		dcb.Init (null,
					(ht_hud) =>
					{
						//TODO lógica de login do jogo
						PhotonNetwork.playerName = username.text;
						PhotonPlayer player      = PhotonNetwork.player;
						Hashtable properties = new Hashtable();
						properties.Add ("ready", false);
						player.SetCustomProperties (properties);

						mainMenu.SetActive (true);
						mainMenu.GetComponent<InternalMainMenu> ().Init ();

						networkGUI.playerName = PhotonNetwork.playerName;

						this.gameObject.SetActive (false);
					});
	}
}
