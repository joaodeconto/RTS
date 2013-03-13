using UnityEngine;
using System.Collections.Generic;

public class ChatMessage : Photon.MonoBehaviour
{

    public static ChatMessage instance;
    public List<string> messages = new List<string>();

    private int chatHeight = 140;
    private int chatWidth = 560;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";
	private bool enableChat;

    void Awake()
    {
        instance = this;
    }
	
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.C)) enableChat = !enableChat;
	}

    void OnGUI()
    {
		if (!enableChat) return;
		
        GUILayout.BeginArea(new Rect((Screen.width / 2) - (chatWidth / 2), (Screen.height / 2) - (chatHeight / 2), chatWidth, chatHeight));
        
		GUILayout.BeginVertical ("box");
		
        //Show scroll list of chat messages
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(messages[i]);
        }
        GUILayout.EndScrollView();

        //Chat input
        chatInput = GUILayout.TextField(chatInput);

        //Group target buttons
        GUILayout.BeginHorizontal();
        GUILayout.Label("Send to:", GUILayout.Width(60));
        if (GUILayout.Button("ALL", GUILayout.Height(17)))
			if (chatInput != "")
	            SendChat(PhotonTargets.All);
				
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
            if (GUILayout.Button("" + player, GUILayout.MaxWidth(100), GUILayout.Height(17)))
			{
				if (chatInput != "")
	                SendChat(player);
			}
		}
        GUILayout.EndHorizontal();
		
		GUILayout.EndVertical ();

        GUILayout.EndArea();
    }

    public static void AddMessage(string text)
    {
        instance.messages.Add(text);
        if (instance.messages.Count > 15)
            instance.messages.RemoveAt(0);
    }


    [RPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[" + info.sender + "] " + text);
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
        chatInput = "";
    }
}
