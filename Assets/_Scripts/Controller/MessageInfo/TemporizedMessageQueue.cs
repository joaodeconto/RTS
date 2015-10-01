using UnityEngine;
using System.Collections;

public class TemporizedMessageQueue : MessageQueue
{
	protected bool overCapLimit;
	private int capCounter;
	public Color fadeColor;
	public float timeToFadeout;
	public int messageCap = 1;

	public TemporizedMessageQueue Init ( GameObject pref_button,
										 UIGrid uiGrid,
										 string queueName,
										 Vector2 rootPosition,
										 Vector2 cellSize,
										 Vector2 padding,
										 float labelSize,
										 bool IsVerticalQueue,
										 float timeToFadeout,
										 Color fadeColor,
	                                     int messageCap,
										 int maxPerLine,
										 int maxItems)
	{
		this.Pref_button = pref_button;
		this.uiGrid = uiGrid;
		this.QueueName       = queueName;
		this.RootPosition    = RootPosition;
		this.IsVerticalQueue = IsVerticalQueue;
		this.MaxPerLine      = maxPerLine;
		this.MaxItems        = maxItems;
		this.CellSize        = cellSize;
		this.Padding         = padding;
		this.LabelSize       = labelSize;
		this.uiGrid.maxPerLine = maxPerLine;
		this.uiGrid.hideInactive = false;
		this.timeToFadeout = timeToFadeout;
		this.fadeColor     = fadeColor;
		this.messageCap    = messageCap;

		return this;
	}

	public override void AddMessageInfo(string buttonName,
										Hashtable ht,
										DefaultCallbackButton.OnClickDelegate onClick = null,
										DefaultCallbackButton.OnPressDelegate onPress = null,
										DefaultCallbackButton.OnSliderChangeDelegate onSliderChange = null,
										DefaultCallbackButton.OnActivateDelegate onActivate = null,
										DefaultCallbackButton.OnRepeatClickDelegate onRepeatClick = null,
										DefaultCallbackButton.OnDragDelegate onDrag = null,
										DefaultCallbackButton.OnDropDelegate onDrop = null)
	{


		if (capCounter>messageCap)
		{
			DeleteFirst();		
			return;
		}

		if(IsEmpty())
		{
			Invoke ("CleanFirstMessage", timeToFadeout);
		}

		base.AddMessageInfo (buttonName, ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);

		capCounter++;

	}

	private void DeleteFirst()
	{
		capCounter--;
		DequeueMessageInfo();
	}

	private void CleanFirstMessage()
	{
		capCounter--;
		DequeueMessageInfo();
		if(!IsEmpty())
		{
			Invoke ("CleanFirstMessage", timeToFadeout);

		}
	}
}
