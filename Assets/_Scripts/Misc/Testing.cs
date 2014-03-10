using UnityEngine;
using System.Collections;

using Visiorama;

public class Testing : MonoBehaviour
{
	public string username = "matheus";
	public string password = "123";
	public string idFacebook = "";
	public string errorMessage;

	public string testNameValue = "test-data";
	public int testValue    =  2;
	public int testBattleId = -3;
	
	public bool test = true;

	void Update () {
		if (test)
		{
			test = false;	
			PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
			
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
						
						Score.LoadScores (
							(dicScore) => 
							{
								Debug.Log ("chegou here");
								Score.AddScorePoints (testNameValue, testValue);
								Score.AddScorePoints (testNameValue,  testValue, testBattleId);
								Score.Save ();
							}
						);
					}
			});
		}
	}
}
