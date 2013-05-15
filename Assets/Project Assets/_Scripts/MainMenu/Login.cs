using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	public Player player;

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
			PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
			pw.SetPlayer (username, true);
			
			EnterInternalMainMenu (username);
			return;
		}

		ComponentGetter.Get<DAOPlayer>().GetPlayer (username, password, idFacebook,
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

				EnterInternalMainMenu (username);
			}
		});
	}

	public void EnterInternalMainMenu (string username)
	{
		HideAllViews ();

		InternalMainMenu imm = ComponentGetter.Get <InternalMainMenu> ();
		imm.Init (username);
	}

	public void DoNewAccount (Hashtable ht)
	{
		string username = (string)ht["username"];
		string password = (string)ht["password"];
		string idFacebook = "";
		string email    = (string)ht["email"];

		ComponentGetter.Get<DAOPlayer>().CreatePlayer (username, password, idFacebook, email,
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
