using UnityEngine;
using System.Collections;

public class RTSPersonalizedCallbackButton : DefaultCallbackButton
{
	public override void Init(Hashtable ht,
					 OnClickDelegate onClick = null,
					 OnPressDelegate onPress = null,
					 OnSliderChangeDelegate onSliderChangeDelegate = null,
					 OnActivateDelegate onActivateDelegate = null,
					 OnRepeatClickDelegate onRepeatClickDelegate = null,
					 OnDragDelegate onDrag = null,
					 OnDropDelegate onDrop = null)
	{
		base.Init(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);
		ChangeParams(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);
	}

	public override void ChangeParams(Hashtable ht,
							 OnClickDelegate onClick = null,
							 OnPressDelegate onPress = null,
							 OnSliderChangeDelegate onSliderChangeDelegate = null,
							 OnActivateDelegate onActivateDelegate = null,
							 OnRepeatClickDelegate onRepeatClickDelegate = null,
							 OnDragDelegate onDrag = null,
							 OnDropDelegate onDrop = null)
	{
		base.ChangeParams(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);

		Transform trnsLabel = transform.FindChild("Label");
		UILabel l = trnsLabel.GetComponent<UILabel>();
		l.text = "";

		if( !ht.ContainsKey("textureName") )
		{
			transform.FindChild("Foreground").GetComponent<UISprite>().enabled = false;
			transform.FindChild("Background").GetComponent<UISprite>().enabled = false;
			trnsLabel.localPosition = Vector3.zero;
		}
		else
		{
			string textureName = (string)ht["textureName"];
			if ( !string.IsNullOrEmpty(textureName) )
			{
				Transform trnsForeground = transform.FindChild("Foreground");
				ChangeButtonForegroundTexture(trnsForeground, textureName);
			}
		}

		if ( ht.ContainsKey("message") )
		{
			l.text = (string)ht["message"];
			if (ht.ContainsKey( "LabelSize" ))
			{
				l.transform.localScale = Vector3.one * (float)ht["LabelSize"];
			}
		}

		trnsLabel = transform.FindChild("Label Counter");
		l = trnsLabel.GetComponent<UILabel>();
		l.text = "";

		Transform trnsBackCounter = transform.FindChild("Background Counter");

		if ( ht.ContainsKey("counter") )
		{
			trnsBackCounter.GetComponent<UISprite>().enabled = true;
			l.text = ((int)ht["counter"]).ToString ();
		}
		else
			trnsBackCounter.GetComponent<UISprite>().enabled = false;


		trnsLabel = transform.FindChild("Label Price");
		l = trnsLabel.GetComponent<UILabel>();
		l.text = "";

		if ( ht.ContainsKey("price") )
			l.text = ((int)ht["price"]).ToString ();
	}

	private void ChangeButtonForegroundTexture(Transform trnsForeground, string textureName)
	{
		if(trnsForeground == null || trnsForeground.GetComponent<UISprite>() == null)
		{
			Debug.LogError("Eh necessario que tenha o objeto \"Foreground\" com um sliced sprite dentro");
			Debug.Break();
			return;
		}

		UISprite sprite = trnsForeground.GetComponent<UISprite>();
		sprite.spriteName = textureName;
		sprite.MakePixelPerfect();
		sprite.transform.localPosition = Vector3.forward * -5;
	}
}
