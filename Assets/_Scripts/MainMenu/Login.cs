using UnityEngine;
using System.Collections;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	public int NumberOfCoinsNewPlayerStartsWith = 10;

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

				Debug.Log ("player: " + player);

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
		
		PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
		
        playerDao.CreatePlayer (username, password, idFacebook, email,
		(player, message) =>
		{
			if (player == null)
			{
				Debug.Log ("Problema na criação do player");
			}
			else
			{
				Debug.Log ("Novo DB.Player");

				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				
				pw.SetPlayer (username, true);
                pw.SetPropertyOnPlayer ("player", player.ToString ());

				Score.SetScorePoints (DataScoreEnum.CurrentCrystals, NumberOfCoinsNewPlayerStartsWith);
				Score.SetScorePoints (DataScoreEnum.TotalCrystals,   NumberOfCoinsNewPlayerStartsWith);
				Score.Save ();
				
				Index ();
			}
		});
	}
}
