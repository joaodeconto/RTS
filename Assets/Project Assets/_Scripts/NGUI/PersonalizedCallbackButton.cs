using UnityEngine;
using System.Collections;

public class PersonalizedCallbackButton : DefaultCallbackButton
{
	public void Init(Hashtable ht,
					 OnClickDelegate onClick = null,
					 OnPressDelegate onPress = null,
					 OnDragDelegate onDrag = null,
					 OnDropDelegate onDrop = null)
	{
		base.Init(ht, onClick, onPress, onDrag, onDrop);
		ChangeParams(ht, onClick, onPress, onDrag, onDrop);
	}

	public void ChangeParams(Hashtable ht,
							 OnClickDelegate onClick = null,
							 OnPressDelegate onPress = null,
							 OnDragDelegate onDrag = null,
							 OnDropDelegate onDrop = null)
	{
		base.ChangeParams(ht, onClick, onPress, onDrag, onDrop);

		Transform trnsLabel = transform.FindChild("Label");
		UILabel l = trnsLabel.GetComponent<UILabel>();
		l.text = "";

		if( !ht.ContainsKey("textureName") )
		{
			transform.FindChild("Foreground").GetComponent<UISlicedSprite>().enabled = false;
			transform.FindChild("Background").GetComponent<UISlicedSprite>().enabled = false;
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
			trnsBackCounter.GetComponent<UISlicedSprite>().enabled = true;
			l.text = ((int)ht["counter"]).ToString ();
		}
		else
			trnsBackCounter.GetComponent<UISlicedSprite>().enabled = false;


		trnsLabel = transform.FindChild("Label Price");
		l = trnsLabel.GetComponent<UILabel>();
		l.text = "";

		if ( ht.ContainsKey("price") )
			l.text = ((int)ht["price"]).ToString ();
	}

	private void ChangeButtonForegroundTexture(Transform trnsForeground, string textureName)
	{
		if(trnsForeground == null || trnsForeground.GetComponent<UISlicedSprite>() == null)
		{
			Debug.LogError("Eh necessario que tenha o objeto \"Foreground\" com um sliced sprite dentro");
			Debug.Break();
			return;
		}

		UISlicedSprite sprite = trnsForeground.GetComponent<UISlicedSprite>();
		sprite.spriteName = textureName;
		sprite.MakePixelPerfect();
		sprite.transform.localPosition = Vector3.forward * -5;
	}
}
