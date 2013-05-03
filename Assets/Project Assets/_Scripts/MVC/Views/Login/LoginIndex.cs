using UnityEngine;
using System.Collections;

using Visiorama;

public class LoginIndex : IView
{
	public UIInput username;
	public UIInput password;
	public UILabel errorMessage;
	public GameObject submitButton;

	public GameObject NewAccountButton;

	// Use this for initialization
	public void Init ()
	{
		errorMessage.enabled = false;

		submitButton
			.AddComponent<DefaultCallbackButton>()
			.Init (null,
			(ht_hud) =>
			{
				//TODO lógica de login do jogo
				if (string.IsNullOrEmpty(username.text) ||
					string.IsNullOrEmpty(password.text))
					return;

				Hashtable ht = new Hashtable ();
				ht["username"] = username.text;
				ht["password"] = password.text;

				controller.SendMessage ("DoLogin", ht, SendMessageOptions.DontRequireReceiver );
			});

		NewAccountButton
			.AddComponent<DefaultCallbackButton>()
			.Init (null,
			(ht_hud) =>
			{
				controller.SendMessage ("NewAccount", SendMessageOptions.DontRequireReceiver );
			});
	}

	public void ShowErrorMessage ()
	{
		errorMessage.enabled = true;
		errorMessage.text = "Wrong password or login";
		Invoke ("CloseErrorMessage", 5.0f);
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
