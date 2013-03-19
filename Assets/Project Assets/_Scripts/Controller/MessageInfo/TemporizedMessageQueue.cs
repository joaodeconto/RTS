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
										 bool IsVerticalQueue,
										 float timeToFadeout,
										 Color fadeColor,
										 int maxPerLine,
										 int maxItems)
	{
		this.Pref_button = pref_button;
		this.QueueName   = queueName;

		this.RootPosition    = RootPosition;
		this.IsVerticalQueue = IsVerticalQueue;
		this.MaxPerLine      = maxPerLine;
		this.MaxItems        = maxItems;

		this.uiGrid = uiGrid;

		this.uiGrid.cellWidth  = cellSize.x;
		this.uiGrid.cellHeight = cellSize.y;
		this.uiGrid.maxPerLine = maxPerLine;
		this.uiGrid.sorted = true;

		this.uiGrid.arrangement = (IsVerticalQueue) ?
										UIGrid.Arrangement.Vertical :
										UIGrid.Arrangement.Horizontal;
		return this;
	}

	public void AddMessageInfo( string buttonName,
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
