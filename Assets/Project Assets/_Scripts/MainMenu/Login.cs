using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	
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

		if (!UseRealLogin)
		{
			EnterInternalMainMenu (username);
			return;
		}

		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { szName = username,
												szPassword = password };

		db.Read (dbPlayer,
		(response) =>
		{
			dbPlayer = response as DB.Player;

			if (dbPlayer == null ||
				string.IsNullOrEmpty(dbPlayer.szName) ||
				string.IsNullOrEmpty(dbPlayer.szPassword))
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
		Debug.Log("DoNewAccount");

		string username = (string)ht["username"];
		string password = (string)ht["password"];
		string email    = (string)ht["email"];

		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { szName     = username,
												szPassword = password,
												szEmail    = email};
		db.Create (dbPlayer,
		(response) =>
		{
			dbPlayer = response as DB.Player;
			if (dbPlayer == null)
			{
				Debug.Log ("Probrema na criação do prayer");
			}
			else
			{
				Debug.Log ("Novo Player");

				Index ();
			}
		});
	}
}
