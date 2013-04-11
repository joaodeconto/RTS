using UnityEngine;
using System.Collections;

public class TemporizedMessageQueue : MessageQueue
{
	public Color fadeColor;
	public float timeToFadeout;

	public TemporizedMessageQueue Init ( GameObject pref_button,
										 UIGrid uiGrid,
										 string queueName,
										 Vector2 rootPosition,
										 Vector2 cellSize,
										 Vector2 padding,
										 float labelSize,
										 bool isVerticalQueue,
										 float timeToFadeout,
										 Color fadeColor,
										 int maxPerLine,
										 int maxItems)
	{
		base.Init ( pref_button,
					uiGrid,
					queueName,
					rootPosition,
					cellSize,
					padding,
					labelSize,
					isVerticalQueue,
					maxPerLine,
					maxItems);

		this.timeToFadeout = timeToFadeout;
		this.fadeColor     = fadeColor;

		return this;
	}

	public override void AddMessageInfo(string buttonName,
										Hashtable ht,
										DefaultCallbackButton.OnClickDelegate onClick = null,
										DefaultCallbackButton.OnPressDelegate onPress = null,
										DefaultCallbackButton.OnDragDelegate onDrag = null,
										DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		if(IsEmpty())
		{
			Invoke ("CleanFirstMessage", timeToFadeout);
		}

		base.AddMessageInfo (buttonName, ht, onClick, onPress, onDrag, onDrop);
	}

	private void CleanFirstMessage()
	{
		DequeueMessageInfo();
		if(!IsEmpty())
		{
			Invoke ("CleanFirstMessage", timeToFadeout);
		}
	}
}
