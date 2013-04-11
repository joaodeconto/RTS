using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : MonoBehaviour
{
	public UIInput username;
	public UIInput password;
	public GameObject submitButton;
	public GameObject mainMenu;

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

		DefaultCallbackButton dcb = submitButton.AddComponent<DefaultCallbackButton>();

		Hashtable ht = new Hashtable();
		dcb.Init ().Show (null,
					(ht_hud) =>
					{
						//TODO lógica de login do jogo
						PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();

						pw.SetPlayer (username.text, true);

						mainMenu.SetActive (true);
						mainMenu.GetComponent<InternalMainMenu> ().Init ();

						this.gameObject.SetActive (false);
					});
	}
}
