using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : MonoBehaviour
{
	public UIInput username;
	public UIInput password;
	public UILabel errorMessage;
	public GameObject submitButton;
	public GameObject mainMenu;

	public void Start ()
	{
		Init ();
	}

	private bool wasInitialized = false;
	public void Init ()
	{
		PhotonNetwork.networkingPeer.DisconnectTimeout = 30000;
		
		if (wasInitialized) return;

		wasInitialized = true;

		Application.runInBackground = true;
		
		errorMessage.enabled = false;
		
		DefaultCallbackButton dcb = submitButton.AddComponent<DefaultCallbackButton>();

		Hashtable ht = new Hashtable();
		dcb.Init (null,
					(ht_hud) =>
					{
						//TODO lógica de login do jogo
						if (!string.IsNullOrEmpty(username.text))
						{
							PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				
							pw.SetPlayer (username.text, true);
	
							mainMenu.SetActive (true);
							mainMenu.GetComponent<InternalMainMenu> ().Init (username.text);
	
							this.gameObject.SetActive (false);
						}
						else
						{
							errorMessage.enabled = true;

							Invoke ("CloseErrorMessage", 5.0f);
						}
					});
	}
	
	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
