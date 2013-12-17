using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class NewAccount : IView
{
	public UIInput username;
	public UIInput email;
	public UIInput password;
	public UIInput password_confirmation;
	public UIToggle AcceptedTerms;
	public UIToggle ReceiveNewsletter;

	public GameObject VerifyAccountNameButton;
	public GameObject CreateAccountButton;

	private bool AccountAlreadyExists;

	public NewAccount Init ()
	{
		CreateAccountButton
			.AddComponent<DefaultCallbackButton> ()
			.Init (null,
			(ht_dcb) =>
			{
				if (AccountAlreadyExists)
					Debug.Log("AccountAlreadyExists");//TODO mostrar que conta já existe

				if (string.IsNullOrEmpty (password.text))
					Debug.Log ("empty password");//TODO campo de password vazio

				if (string.IsNullOrEmpty (password_confirmation.text) || password.text != password_confirmation.text)
					Debug.Log("you need confirm password ");//TODO mostrar password não confirmado

				if (string.IsNullOrEmpty (username.text) || username.text.Length < 6)
					Debug.Log("invalid username");//TODO username inválido

				if (!Regex.Match(email.text, @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$").Success)
					Debug.Log("invalid e-mail");//TODO e-mail inválido

				Hashtable ht = new Hashtable ();
				ht["username"]              = username.text;
				ht["password"]              = password.text;
				ht["email"]                 = email.text;

				controller.SendMessage ("DoNewAccount", ht, SendMessageOptions.DontRequireReceiver );
			});

		VerifyAccountNameButton
			.AddComponent<DefaultCallbackButton> ()
			.Init (null,
			(ht_dcb) =>
			{
				//TODO fazer verificação
				AccountAlreadyExists = (Random.value % 2 == 1) ? true : false;

				if (AccountAlreadyExists)
					Debug.Log("AccountAlreadyExists");//TODO mostrar que conta já existe
			});

		return this;
	}
}
