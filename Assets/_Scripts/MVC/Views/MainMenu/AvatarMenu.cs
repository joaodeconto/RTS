using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Utils;

public class AvatarMenu : MonoBehaviour
{
	private bool wasInitialized = false;
	public UISprite avatarImg;
	public Transform buttons;
	private List<Transform> avatarList;
	
	public void OnEnable ()
	{
		Open ();
	}
					
	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;				
		DefaultCallbackButton dcb;

		avatarList = new List<Transform>();
		foreach (Transform button in buttons)
		{
			avatarList.Add (button);
			Transform trnsAvatar = button.FindChild("Sprite (AVATAR)");
			Hashtable ht = new Hashtable ();
			ht["avatarImg"] = trnsAvatar.GetComponent<UISprite>().spriteName;
			dcb = button.gameObject.AddComponent<DefaultCallbackButton>();
			dcb.Init(ht, (ht_dcb)=>{					
				avatarImg.spriteName = (string)ht_dcb["avatarImg"];
				PlayerPrefs.SetString("Avatar", avatarImg.spriteName);
			});

		}
		
		Transform close = this.transform.FindChild ("Menu").FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{
				gameObject.SetActive (false);
			});
		}
	}

	void Close()
	{
		gameObject.SetActive (false);
	}
}