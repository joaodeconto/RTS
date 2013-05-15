using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	public Model.Player player;

	public void Start ()
	{
		Init ();
	}

	public void Init ()
	{
		ComponentGetter.Get<PhotonWrapper> ().Init ();

		CheckAllViews ();

		Index ();
	}

	public void Index ()
	{
		HideAllViews ();

		LoginIndex index = GetView <LoginIndex> ("Index");
		index.SetActive (true);
		index.Init ();
	}

	public void NewAccount ()
	{
		HideAllViews ();

		NewAccount newAccount = GetView <NewAccount> ("NewAccount");
		newAccount.SetActive (true);
		newAccount.Init ();
	}

	public void DoLogin (Hashtable ht)
	{
		string username = (string)ht["username"];
		string password = (string)ht["password"];
		string idFacebook = "";

		if (!UseRealLogin)
		{
			EnterInternalMainMenu (username);
			return;
		}

		ComponentGetter.Get<PlayerDAO>().GetPlayer (username, password, idFacebook,
		(player, message) =>
		{
			this.player = player;

			if (player == null)
			{
				LoginIndex index = GetView <LoginIndex> ("Index");
				index.ShowErrorMessage ();
			}
			else
			{
				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				pw.SetPlayer (username, true);
				pw.SetPropertyOnPlayer ("player", player.ToString ());

				EnterInternalMainMenu (username);
			}
		});
	}

	public void EnterInternalMainMenu (string username)
	{
		HideAllViews ();

		InternalMainMenu imm = ComponentGetter.Get <InternalMainMenu> ();
		imm.Init (player);
	}

	public void DoNewAccount (Hashtable ht)
	{
		Debug.Log("DoNewAccount");

		string username = (string)ht["username"];
		string password = (string)ht["password"];
		string idFacebook = "";
		string email    = (string)ht["email"];

		ComponentGetter.Get<PlayerDAO>().CreatePlayer (username, password, idFacebook, email,
		(player, message) =>
		{
			if (player == null)
			{
				Debug.Log ("Problema na criação do player");
			}
			else
			{
				Debug.Log ("Novo DB.Player");

				Index ();
			}
		});
	}
}
