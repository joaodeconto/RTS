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

		public int IdPlayer;
		public string SzName;
		public string IdFacebookAccount;
		public string SzPassword;
		public string TmTimePlayed;
		public DateTime DtAccountCreated;
		public DateTime DtAccountUpdated;
		public int IdCountry;
	}

	public TestPlayer testPlayer;

	DB.Player dbPlayer;
	string message;

	void Start ()
	{
		Database db = ComponentGetter.Get <Database> ();
		dbPlayer = new DB.Player ();

		dbPlayer.IdPlayer          = testPlayer.IdPlayer.ToString ();
		dbPlayer.SzName            = testPlayer.SzName;
		dbPlayer.IdFacebookAccount = testPlayer.IdFacebookAccount;
		dbPlayer.SzPassword        = testPlayer.SzPassword;
		dbPlayer.TmTimePlayed      = testPlayer.TmTimePlayed;
		dbPlayer.DtAccountCreated  = System.DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss");//0000-00-00 00:00:00
		dbPlayer.DtAccountUpdated  = System.DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss");
		dbPlayer.IdCountry         = testPlayer.IdCountry.ToString ();

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
			testPlayer.IdPlayer		      = dbPlayer.IdPlayer != "null" ? int.Parse (dbPlayer.IdPlayer) : 0;
			testPlayer.SzName             = dbPlayer.SzName;
			testPlayer.IdFacebookAccount  = dbPlayer.IdFacebookAccount;
			testPlayer.SzPassword         = dbPlayer.SzPassword;
			testPlayer.TmTimePlayed       = dbPlayer.TmTimePlayed;
			//testPlayer.dtAccountCreated = dbPlayer.dtAccountCreated;
			//testPlayer.dtAccountUpdated = dbPlayer.dtAccountUpdated;
			//testPlayer.idCountry		  = (dbPlayer.idCountry != "null") ? int.Parse(dbPlayer.idCountry) : 0;
		}
	}
}
