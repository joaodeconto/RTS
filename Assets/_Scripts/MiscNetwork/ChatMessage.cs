using UnityEngine;
using System.Collections.Generic;
using Visiorama;

public class ChatMessage : Photon.MonoBehaviour
{

    public static ChatMessage instance;
	
	public List<int> teamIdMessage = new List<int>();
    public List<string> messages = new List<string>();

    private int chatHeight = 140;
    private int chatWidth = 360;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";
	private bool enableChat, focusTextField, activeTextField;
	
	protected GameplayManager gameplayManager;

    void Awake()
    {
        instance = this;
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
    }
	
//    void OnGUI()
//    {
////		Debug.Log ("Screen.width/1280: " + (Screen.width/1280) + " - 1280/Screen.width: " + (1280/Screen.width));
//        GUILayout.BeginArea(new Rect(0, (Screen.height / 2) - (chatHeight / 2), chatWidth * ((float)Screen.width/1280f), chatHeight));
//        
//        //Show scroll list of chat messages
//        scrollPos = GUILayout.BeginScrollView(scrollPos);
//        for (int i = messages.Count - 1; i >= 0; i--)
//        {
//			GUI.color = gameplayManager.GetColorTeam(teamIdMessage[i]);
//            GUILayout.Label(messages[i]);
//        }
//        GUILayout.EndScrollView();
//		
//		GUI.color = Color.white;
//		
//		if (enableChat)
//		{
//	        //Chat input
//			GUI.SetNextControlName ("ChatInput");
//	        chatInput = GUILayout.TextField(chatInput);
//			
//			if (!focusTextField)
//			{
//				GUI.FocusControl ("ChatInput");
//				focusTextField = true;
//			}
//			
//	        //Group target buttons
//	        GUILayout.BeginHorizontal();
////	        GUILayout.Label("Send to:", GUILayout.Width(60));
//			
////	        if (GUILayout.Button("Send", GUILayout.Height(17)))
////				if (chatInput != "")
////		            SendChat(PhotonTargets.All);
//			
////	        foreach (PhotonPlayer player in PhotonNetwork.playerList)
////			{
////	            if (GUILayout.Button("" + player, GUILayout.MaxWidth(100), GUILayout.Height(17)))
////				{
////					if (chatInput != "")
////		                SendChat(player);
////				}
////			}
//			
//			if (Event.current.keyCode == KeyCode.Return &&
//				Event.current.type == EventType.keyUp)
//			{
//				if (!activeTextField) 
//				{
//					activeTextField = true;
//				}
//				else
//				{
//					enableChat = focusTextField = false;
//					if (chatInput != "")
//			            SendChat(PhotonTargets.All);
//				}
//	        }
//			
//	        GUILayout.EndHorizontal();
//		}
//		else
//		{
//			if (Event.current.Equals (Event.KeyboardEvent ("return")))
//			{
//				enableChat = true;
//				activeTextField = false;
//	        }
//		}
//        GUILayout.EndArea();
//    }

    public static void AddMessage(string text, int teamId)
    {
        instance.messages.Insert(0, text);
		instance.teamIdMessage.Insert(0, teamId);
        if (instance.messages.Count > 4)
		{
			instance.teamIdMessage.RemoveAt(instance.teamIdMessage.Count-1);
            instance.messages.RemoveAt(instance.messages.Count-1);
		}
		instance.scrollPos += (Vector2.up * instance.chatHeight * 15);
    }

    [RPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
		int teamId = (int)info.sender.customProperties["team"];
        AddMessage("[" + info.sender + "] " + text, teamId);
    }

    void SendChat(PhotonTargets target)
    {
        photonView.RPC("SendChatMessage", target, chatInput);
        chatInput = "";
    }

    void SendChat(PhotonPlayer target)
    {
        chatInput = "(P) " + chatInput;
        photonView.RPC("SendChatMessage", target, chatInput);
        photonView.RPC("SendChatMessage", PhotonNetwork.player, chatInput);
        chatInput = "";
    }
}
