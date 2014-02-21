using UnityEngine;
using System.Collections;

public class FacebookLoginHandler : MonoBehaviour
{
	public System.Func <bool> OnLoggedIn;
	
	private void Awake ()
	{
		enabled = false;
		FB.Init (OnInitComplete, OnHideUnity);
	}
	
	private void OnInitComplete ()                                                                       
	{                           
		Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);                                                                 
		enabled = true; // "enabled" is a property inherited from MonoBehaviour                  
		if (FB.IsLoggedIn)
		{                                                                                        
			FbDebug.Log("Already logged in");
			FbDebug.Log ("AppId: " + FB.AppId + " - FB.UserId: " + FB.UserId );
		}                                                                                        
	}                                                                                            
	
	private void OnHideUnity(bool isGameShown)                                                   
	{                                                                                            
		FbDebug.Log("OnHideUnity");
		if (!isGameShown)
		{                                                                                        
			// pause the game - we will need to hide                                             
			Time.timeScale = 0;                                                                  
		}                                                                                        
		else                                                                                     
		{                                                                                        
			// start the game back up - we're getting focus again                                
			Time.timeScale = 1;                                                                  
		}                                                                                        
	}
	
	void LoginCallback(FBResult result)                                                        
	{                                                                                          
		FbDebug.Log("LoginCallback");                                                          
		
		if (FB.IsLoggedIn)                                                                     
		{                                                                                      
			OnLoggedIn();                                                                      
		}                                                                                      
	}   

	public FacebookLoginHandler DoLogin ()
	{                                                        
		FbDebug.Log("DoLogin");  
		FB.Login("email,publish_actions", LoginCallback);
		
		return this;
	}
}
