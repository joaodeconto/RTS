using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using I2.Loc;

public class VersusScreen : MonoBehaviour
{
	public static Dictionary<string, string> opponentSprite = new Dictionary<string, string>();
	public static string modeLabelString;
	public static string mapLabelString;

	[System.Serializable]
	public class ConfigurationOfScreen
	{
		public GameplayManager.Mode mode;
		public int numberOfPlayers;
		public Vector3[] positions;
	}
	public ConfigurationOfScreen[] configurationOfScreen;
	public GameObject gameObjectPlayerL;
	public GameObject gameObjectPlayerR;
	public GameObject prefabPlayerRight;
	public GameObject prefabPlayerLeft;
	public GameObject goVersusScreen;	
	public UILabel battleMode;
	public UISprite mapSprite;
	public UISlider progBar;
	public UILabel mapName;
	public int cena;	

	protected PhotonWrapper pw;
	protected int timeCount;

	void Awake ()
	{
		pw = ComponentGetter.Get<PhotonWrapper> ();		
		pw.SetStartGame (() => InitOnlineGame ());
	}
	
	public void InitOnlineGame ()
	{		
		opponentSprite.Clear();
		cena = (int)pw.GetPropertyOnRoom("map");
		cenaSelection();
		ComponentGetter.Get<InternalMainMenu> ().goMainMenu.SetActive (false);
		goVersusScreen.SetActive (true);
		int totalPlayers = PhotonNetwork.playerList.Length;		
		int configurationOfScreenSelected = 0;		
		int i = 0;
		foreach (ConfigurationOfScreen cos in configurationOfScreen)
		{
			if (GameplayManager.mode == cos.mode){
				if (cos.numberOfPlayers == totalPlayers){
					configurationOfScreenSelected = i;
					break;
				}
			}			
			i++;
		}
		
		if (GameplayManager.mode == GameplayManager.Mode.Cooperative){
			modeLabelString  = "cooperative";
			battleMode.text = ScriptLocalization.Get("Menus/cooperative"); 
			int ally = (int)PhotonNetwork.player.customProperties["allies"];			
			int k = totalPlayers / 2;			
			i = 0;

			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				opponentSprite.Add((string)pp.customProperties["name"], (string)pp.customProperties["avatar"]);

				if ((int)pp.customProperties["allies"] == ally){
					SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[i], pp);					
					i++;
				}
				else{
					SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[k], pp);					
					k++;
				}
			}
		}
		else if (GameplayManager.mode == GameplayManager.Mode.Tutorial){
			modeLabelString  = "Single Player"; 
			battleMode.text = ScriptLocalization.Get("Menus/Single Player"); 
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[i], pp);				
				i++;
			}
		}		
		else{
			battleMode.text = ScriptLocalization.Get("Menus/Deathmatch");
			modeLabelString  = "DeathMatch";
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				SetPlayer (configurationOfScreen[configurationOfScreenSelected].positions[(int)pp.customProperties["team"]], pp);				
				i++;
			}
		}
		Invoke ("InstanceGame",2);
		mapLabelString 	= mapName.text;
	}

	public void InitOfflineGame (int maxPlayers, int bid, string battleTypeName, int map, int level)
	{
		if(PhotonNetwork.connected) PhotonNetwork.Disconnect();
		PhotonNetwork.offlineMode = true;
		GameplayManager.mode = GameplayManager.Mode.Tutorial;
		opponentSprite.Clear();
		cena = map+1;
		cenaSelection();
		if (!ConfigurationData.Offline)	ComponentGetter.Get<InternalMainMenu> ().goMainMenu.SetActive (false);
		goVersusScreen.SetActive (true);	
		int totalPlayers = 2;
		battleMode.text = ScriptLocalization.Get("Menus/Single Player"); 
		Invoke ("InstanceGame",2);
		mapLabelString 	= mapName.text;
		modeLabelString	= "Single Player";
		ConfigurationData.InGame = true;
		ConfigurationData.level = level;

		//TODO carregar img do player e AI
	}

	public void cenaSelection ()
	{
		if (cena == 8){
			mapName.text = "Lang Lagoon";
			mapSprite.spriteName = "Crank Lagoon";			
		}
		if (cena == 9){
			mapName.text = "Arthanus";
			mapSprite.spriteName = "Arthanus";			
		}
		if (cena == 4){
			mapName.text = "Gargantua";
			mapSprite.spriteName = "Gargantua";			
		}
		if (cena == 6){
			mapName.text = "Sandstone Salvation";
			mapSprite.spriteName = "Sandstone Salvation";			
		}
		if (cena == 3){
			mapName.text = "Hollow Fields";
			mapSprite.spriteName = "Hollow Fields";			
		}
		if (cena == 7){
			mapName.text = "Swamp King";
			mapSprite.spriteName = "Swamp King";
		}
		if (cena == 5){
			mapName.text = "Living Desert";
			mapSprite.spriteName = "Living Desert";			
		}
		if (cena == 2){
			mapName.text = "Dementia Forest";
			mapSprite.spriteName = "Dementia Forest";			
		}
	}	

	void SetPlayer (Vector3 position, PhotonPlayer pp)
	{	
		if (position.x <= 0){
			GameObject button = NGUITools.AddChild (gameObjectPlayerL, prefabPlayerLeft);						
			button.transform.localPosition = position;	
			button.GetComponentInChildren<UILabel> ().text = pp.name;
			string avatarSprite = (string)pp.customProperties["avatar"];
			UISprite buttonAvatar = button.transform.FindChild("GameObject").transform.FindChild("avatar-sprite").gameObject.GetComponentInChildren<UISprite>();
			buttonAvatar.spriteName = avatarSprite;
		}

		else{
			GameObject button = NGUITools.AddChild (gameObjectPlayerR, prefabPlayerRight);						
			button.transform.localPosition = position;			
			button.GetComponentInChildren<UILabel> ().text = pp.name;
			string avatarSprite = (string)pp.customProperties["avatar"]; 
			UISprite buttonAvatar = button.transform.FindChild("GameObject").transform.FindChild("avatar-sprite").gameObject.GetComponentInChildren<UISprite>();
			buttonAvatar.spriteName = avatarSprite;
		}
	}
	
	void InstanceGame ()
	{
		pw.StartGame ();
		StartCoroutine (LoadingScene ());
	}

	private IEnumerator LoadingScene ()
	{
		AsyncOperation async = Application.LoadLevelAsync(cena);
		while (!async.isDone)
		{
			progBar.value = async.progress + 0.1f;
			yield return null;
		}
	}		

}
