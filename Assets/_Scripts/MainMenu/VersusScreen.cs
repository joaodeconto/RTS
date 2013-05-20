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
	public UILabel timeLabel;
	
	public GameObject prefabPlayerVersus;
		
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
		
		if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
		{
			int ally = (int)PhotonNetwork.player.customProperties["allies"];
			
			int k = totalPlayers / 2;
			
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				if ((int)pp.customProperties["allies"] == ally)
				{
					SetPlayer (goPlayers,
						configurationOfScreen[configurationOfScreenSelected].positions[i],
						pp);
					
					i++;
				}
				else
				{
					SetPlayer (goPlayers,
						configurationOfScreen[configurationOfScreenSelected].positions[k],
						pp);
					
					k++;
				}
			}
		}
		else
		{
			i = 0;
			foreach (PhotonPlayer pp in PhotonNetwork.playerList)
			{
				SetPlayer (goPlayers,
					configurationOfScreen[configurationOfScreenSelected].positions[i],
					pp);
				
				i++;
			}
		}
		
		Invoke ("InstanceGame", timeToWait+1);
		InvokeRepeating ("DescountTime", 1f, 1f);
	}
	
	void DescountTime ()
	{
		--timeCount;
		timeLabel.text = timeCount.ToString();
		
		if (timeCount == 0) CancelInvoke ("DescountTime");
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
