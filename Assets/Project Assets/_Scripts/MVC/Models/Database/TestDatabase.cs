using UnityEngine;

using System;
using System.Collections;

using Visiorama;

public class TestDatabase : MonoBehaviour
{
	[Serializable()]
	public class TestPlayer
	{
		public bool TestCreate;
		public bool TestRead;
		public bool TestUpdate;
		public bool TestDelete;

		public int idPlayer;
		public string szName;
		public string idFacebookAccount;
		public string szPassword;
		public string tmTimePlayed;
		public DateTime dtAccountCreated;
		public DateTime dtAccountUpdated;
		public int idCountry;
	}

	public TestPlayer testPlayer;

	DB.Player dbPlayer;
	string message;

	void Start ()
	{
		Database db = ComponentGetter.Get <Database> ();
		dbPlayer = new DB.Player ();

		dbPlayer.idPlayer          = testPlayer.idPlayer.ToString ();
		dbPlayer.szName            = testPlayer.szName;
		dbPlayer.idFacebookAccount = testPlayer.idFacebookAccount;
		dbPlayer.szPassword        = testPlayer.szPassword;
		dbPlayer.tmTimePlayed      = testPlayer.tmTimePlayed;
		dbPlayer.dtAccountCreated  = System.DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss");//0000-00-00 00:00:00
		dbPlayer.dtAccountUpdated  = System.DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss");
		dbPlayer.idCountry         = testPlayer.idCountry.ToString ();

		if (testPlayer.TestCreate)
			db.Create (dbPlayer, CheckPlayer);
		if (testPlayer.TestRead)
			db.Read (dbPlayer, CheckPlayer);
		if (testPlayer.TestUpdate)
			db.Update (dbPlayer, CheckPlayer);
		if (testPlayer.TestDelete)
			db.Delete (dbPlayer, CheckPlayer);
	}

	private void CheckPlayer (object response)
	{
		Debug.Log("Check Player");

		dbPlayer = response as DB.Player;

		Debug.LogError ("dbPlayer: " + dbPlayer);

		if(dbPlayer == null)
		{
			message = response as string;
			Debug.LogWarning ("message: " + message);
		}
		else
		{
			testPlayer.idPlayer		      = dbPlayer.idPlayer != "null" ? int.Parse (dbPlayer.idPlayer) : 0;
			testPlayer.szName             = dbPlayer.szName;
			testPlayer.idFacebookAccount  = dbPlayer.idFacebookAccount;
			testPlayer.szPassword         = dbPlayer.szPassword;
			testPlayer.tmTimePlayed       = dbPlayer.tmTimePlayed;
			//testPlayer.dtAccountCreated = dbPlayer.dtAccountCreated;
			//testPlayer.dtAccountUpdated = dbPlayer.dtAccountUpdated;
			//testPlayer.idCountry		  = (dbPlayer.idCountry != "null") ? int.Parse(dbPlayer.idCountry) : 0;
		}
	}
}
