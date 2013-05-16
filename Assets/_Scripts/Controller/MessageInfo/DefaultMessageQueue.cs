using UnityEngine;
using System.Collections;

public class DefaultMessageQueue : MessageQueue
{
	public DefaultMessageQueue Init(GameObject pref_button,
									UIGrid uiGrid,
									string queueName,
									Vector2 rootPosition,
									Vector2 cellSize,
									Vector2 padding,
									float labelSize,
									bool isVerticalQueue,
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
		this.uiGrid.sorted       = true;
		this.uiGrid.hideInactive = false;

		return this;
	}
}
