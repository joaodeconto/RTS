using UnityEngine;
using System.Collections;

public class DefaultMessageQueue : MessageQueue
{
	public DefaultMessageQueue Init(GameObject pref_button,
									 UIGrid uiGrid,
									 string queueName,
									 Vector2 rootPosition,
									 Vector2 cellSize,
									 bool isVerticalQueue,
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
}
