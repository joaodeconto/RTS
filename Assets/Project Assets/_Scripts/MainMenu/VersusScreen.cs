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
	
	public GameObject prefabPlayerVersus;
	
	public float timeToWait;
	
	public ConfigurationOfScreen[] configurationOfScreen;
	
	protected PhotonWrapper pw;
	
	void Awake ()
	{
		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		pw.SetStartGame (() => Init ());
	}
	
	public void Init ()
	{
		ComponentGetter.Get<InternalMainMenu> ().goMainMenu.SetActive (false);
		goVersusScreen.SetActive (true);
		
		GameObject goPlayers = goVersusScreen.GetComponentInChildren<UIPanel> ().transform.FindChild ("Players").gameObject;
		
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
		
		if (GameplayManager.mode == GameplayManager.Mode.Allies)
		{
			int ally = (int)PhotonNetwork.player.customProperties["allies"];
			
			i = 0;			
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				if ((int)pp.customProperties["allies"] == ally)
				{
					SetPlayer (goPlayers, configurationOfScreen[configurationOfScreenSelected].positions[i], pp);
					
					i++;
				}
			}
		}
		else
		{
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				SetPlayer (goPlayers, configurationOfScreen[configurationOfScreenSelected].positions[i], pp);
				
				i++;
			}
		}
		
		Invoke ("InstanceGame", timeToWait);
	}
	
	void SetPlayer (GameObject goPlayers, Vector3 position, PhotonPlayer pp)
	{
		GameObject button = NGUITools.AddChild (goPlayers, prefabPlayerVersus);
		button.transform.localPosition = position;
		
		button.GetComponentInChildren<UILabel> ().text = pp.name;
	}
	
	void InstanceGame ()
	{
		pw.StartGame ();
	}
}
