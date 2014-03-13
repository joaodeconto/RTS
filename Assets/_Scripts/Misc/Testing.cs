using UnityEngine;
using System.Collections;

using Visiorama;

public class Testing : MonoBehaviour
{
	[System.Serializable()]
	public class FakeUser
	{
		public string username = "matheus";
		public string password = "123";
		public string idFacebook = "";
	}
	
	public FakeUser[] users;
	
	public string errorMessage;

	public string testNameValue = "test-data";
	public int testValue    =  2;
	public int testBattleId = -3;
	
	public bool test = true;
	public bool isTesting = false;
	public int testingIndex = 0;

	private bool testNextUser;

	void Update ()
	{
		if (test || testNextUser)
		{
			test = false;
			isTesting = true;
			testNextUser = false;
			
			PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
			
			string username   = users[testingIndex].username;
			string password   = users[testingIndex].password;
			string idFacebook = users[testingIndex].idFacebook;
			
			playerDao.GetPlayer
			(
				username, password, idFacebook,
			    (player, message) =>
			    {
					ConfigurationData.player = player;
				
					Debug.Log ("player: " + player);
					
					if (player == null)
					{
						errorMessage = "player is null";
					}
					else
					{
						PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
						
						pw.SetPlayer (username, true);
						pw.SetPropertyOnPlayer ("player", player.ToString ());
						
						Score.LoadScores
						(
							() => 
							{
								Debug.Log ("chegou here");
								
								Score.AddScorePoints (testNameValue, testValue);
								Score.AddScorePoints (testNameValue,  testValue, testBattleId);
								
//								Score.SubtractScorePoints (testNameValue, testValue);
//								Score.SubtractScorePoints (testNameValue,  testValue, testBattleId);
//								
//								Score.SetScorePoints (testNameValue, testValue);
//								Score.SetScorePoints (testNameValue,  testValue, testBattleId);
								
								if (++testingIndex == users.Length)
								{
									isTesting = false;
									testingIndex = 0;
								}
								else
								{
									testNextUser = true;
								}
							}
						);
					}
			});
		}
	}
}
