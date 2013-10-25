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
		if (ConfigurationData.Logged) return;
		
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

		PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();

		if (!UseRealLogin)
		{
			ConfigurationData.player = new Model.Player () { IdPlayer = 5,
												SzName			  = username,
												SzPassword		  = password,
												IdFacebookAccount = idFacebook };

			pw.SetPlayer (username, true);
			pw.SetPropertyOnPlayer ("player", ConfigurationData.player.ToString ());
			EnterInternalMainMenu (username);
		}
		else
		{
			PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();

			playerDao.GetPlayer (username, password, idFacebook,
			(player, message) =>
			{
				ConfigurationData.player = player;

				if (player == null)
				{
					LoginIndex index = GetView <LoginIndex> ("Index");
					index.ShowErrorMessage ();
				}
				else
				{
					pw.SetPlayer (username, true);
					pw.SetPropertyOnPlayer ("player", player.ToString ());

					EnterInternalMainMenu (username);
				}
			});
		}
	}

	public void EnterInternalMainMenu (string username)
	{
		ConfigurationData.Logged = true;
		
		HideAllViews ();

		InternalMainMenu imm = ComponentGetter.Get <InternalMainMenu> ();
		imm.Init ();
	}

	public void DoNewAccount (Hashtable ht)
	{
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
