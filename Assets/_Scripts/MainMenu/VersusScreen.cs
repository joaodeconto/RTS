
using UnityEngine;
using System.Collections;
using Visiorama;

public class VersusScreen : MonoBehaviour
{
	[System.Serializable]
	public class ConfigurationOfScreen
	{
		public GameplayManager.Mode mode;
		public int numberOfPlayers;
		public Vector3[] positions;
	}
	
	public GameObject goVersusScreen;
	public GameObject gameObjectPlayerL;
	public GameObject gameObjectPlayerR;

	public UILabel timeLabel;
	public UILabel mapName;
	public UILabel battleMode;

	
	public GameObject prefabPlayerRight;
	public GameObject prefabPlayerLeft;


	public int timeToWait;
	
	public ConfigurationOfScreen[] configurationOfScreen;
	
	protected int timeCount;
	
	protected PhotonWrapper pw;
	
	void Awake ()
	{
		timeCount = timeToWait;
		timeLabel.text = timeCount.ToString ();
		
		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		pw.SetStartGame (() => Init ());
	}
	
	public void Init ()
	{
		ComponentGetter.Get<InternalMainMenu> ().goMainMenu.SetActive (false);
		goVersusScreen.SetActive (true);

		// LOAD GAMEPLAY!

		Invoke ("InstanceGame", timeToWait+1);
		InvokeRepeating ("DescountTime", 1f, 1f);

		int totalPlayers = PhotonNetwork.playerList.Length;
		
		int configurationOfScreenSelected = 0;
		
		int i = 0;
		foreach (ConfigurationOfScreen cos in configurationOfScreen)
		{
			if (GameplayManager.mode == cos.mode)
			{
				if (cos.numberOfPlayers == totalPlayers)
				{
					configurationOfScreenSelected = i;
					break;
				}
			}
			
			i++;
		}
		
		if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
		{
			battleMode.text = ("Ranked Coop"); 

			int ally = (int)PhotonNetwork.player.customProperties["allies"];
			
			int k = totalPlayers / 2;
			
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				if ((int)pp.customProperties["allies"] == ally)
				{
					SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[i], pp);
					
					i++;
				}
				else
				{
					SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[k], pp);
					
					k++;
				}
			}
		}
		else
		{
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[i], pp);
				
				i++;
			}
		}
	}
	
	void DescountTime ()
	{
		--timeCount;
		timeLabel.text = timeCount.ToString();
		
		if (timeCount == 0) CancelInvoke ("DescountTime");
	}
		
	void SetPlayer (Vector3 position, PhotonPlayer pp)
	{

	
		if (position.x <= 0)
		{

			GameObject button = NGUITools.AddChild (gameObjectPlayerL, prefabPlayerLeft);
						
			button.transform.localPosition = position;
			
			button.GetComponentInChildren<UILabel> ().text = pp.name;


		}

		else
		{
			GameObject button = NGUITools.AddChild (gameObjectPlayerR, prefabPlayerRight);
						
			button.transform.localPosition = position;
			
			button.GetComponentInChildren<UILabel> ().text = pp.name;

		}

	}
	
	void InstanceGame ()
	{
		pw.StartGame ();
	}
}
