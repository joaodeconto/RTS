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

		return this;
	}
}
