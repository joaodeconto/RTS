using UnityEngine;
using System.Collections;

public class PersonalizedCallbackButton : DefaultCallbackButton
{
	public PersonalizedCallbackButton Init()
	{
		base.Init ();
		return this;
	}

	public void Show(Hashtable ht,
					 OnClickDelegate onClick = null,
					 OnPressDelegate onPress = null,
					 OnDragDelegate onDrag = null,
					 OnDropDelegate onDrop = null)
	{
		base.Show (ht, onClick, onPress, onDrag, onDrop);

		Transform trnsLabel = transform.FindChild("Label");
		UILabel l;
		UISlicedSprite sprite;

		if (!ht.ContainsKey("textureName"))
		{
			sprite = transform.FindChild("Foreground").GetComponent<UISlicedSprite>();

			if (sprite.enabled)
				sprite.enabled = false;

			sprite = transform.FindChild("Background").GetComponent<UISlicedSprite>();

			if (sprite.enabled)
				sprite.enabled = false;

			trnsLabel.localPosition = Vector3.zero;
		}
		else
		{
			sprite = transform.FindChild("Foreground").GetComponent<UISlicedSprite>();

			if (!sprite.enabled)
				sprite.enabled = true;

			sprite = transform.FindChild("Background").GetComponent<UISlicedSprite>();

			if (!sprite.enabled)
				sprite.enabled = true;

			string textureName = (string)ht["textureName"];
			if (!string.IsNullOrEmpty(textureName))
			{
				Transform trnsForeground = transform.FindChild("Foreground");
				ChangeButtonForegroundTexture(trnsForeground, textureName);
			}
		}

		l = trnsLabel.GetComponent<UILabel>();

		if (ht.ContainsKey("message"))
			l.text = (string)ht["message"];
		else
			l.text = "";

		trnsLabel = transform.FindChild("Label Counter");
		l = trnsLabel.GetComponent<UILabel>();

		Transform trnsBackCounter = transform.FindChild("Background Counter");

		if (ht.ContainsKey("counter"))
		{
			sprite = trnsBackCounter.GetComponent<UISlicedSprite>();

			if (!sprite.enabled)
				sprite.enabled = true;

			l.text = (string)ht["counter"];
		}
		else
		{
			sprite = trnsBackCounter.GetComponent<UISlicedSprite>();

			if (sprite.enabled)
				sprite.enabled = false;

			l.text = "";
		}
	}

	public void Hide ()
	{
		UILabel l;
		UISlicedSprite sprite;

		l = transform.FindChild("Label").GetComponent<UILabel>();
		l.text = "";

		sprite = transform.FindChild("Foreground").GetComponent<UISlicedSprite>();

		if (sprite.enabled)
			sprite.enabled = false;

		sprite = transform.FindChild("Background").GetComponent<UISlicedSprite>();

		if (sprite.enabled)
			sprite.enabled = false;

		l = transform.FindChild("Label Counter").GetComponent<UILabel>();
		l.text = "";

		Transform trnsBackCounter = transform.FindChild("Background Counter");

		sprite = trnsBackCounter.GetComponent<UISlicedSprite>();

		if (sprite.enabled)
			sprite.enabled = false;
	}

	private void ChangeButtonForegroundTexture(Transform trnsForeground, string textureName)
	{
		if(trnsForeground == null || trnsForeground.GetComponent<UISlicedSprite>() == null)
		{
			Debug.LogError("Eh necessario que tenha o objeto \"Foreground\" com um sliced sprite dentro");
			Debug.Break();
		}

		UISlicedSprite sprite = trnsForeground.GetComponent<UISlicedSprite>();
		sprite.spriteName = textureName;
		sprite.MakePixelPerfect();
		sprite.transform.localPosition = Vector3.forward * -5;
	}
}
