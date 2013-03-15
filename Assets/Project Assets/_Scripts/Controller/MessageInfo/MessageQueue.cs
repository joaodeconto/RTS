using UnityEngine;
using System.Collections;

public class MessageQueue : MonoBehaviour
{
	public string queueName;
	public Vector2 rootPosition;
	public bool IsVerticalQueue;
	public int maxPerLine;
	public int maxItems;

	private GameObject pref_button;
	private UIGrid uiGrid;

	private float nQueueItems;

	public MessageQueue Init(GameObject pref_button,
							 UIGrid uiGrid,
							 string queueName,
							 Vector2 rootPosition,
							 bool IsVerticalQueue,
							 int maxPerLine,
							 int maxItems)
	{
		this.pref_button = pref_button;
		this.uiGrid      = uiGrid;

		this.queueName       = queueName;
		this.rootPosition    = rootPosition;
		this.IsVerticalQueue = IsVerticalQueue;
		this.maxPerLine      = maxPerLine;
		this.maxItems        = maxItems;

		this.nQueueItems = 0;

		return this;
	}

	public void AddMessageInfo( string buttonName,
								Hashtable ht,
								string textureName,
								DefaultCallbackButton.OnClickDelegate onClick = null,
								DefaultCallbackButton.OnPressDelegate onPress = null,
								DefaultCallbackButton.OnDragDelegate onDrag = null,
								DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		++nQueueItems;

		GameObject button = NGUITools.AddChild (uiGrid.gameObject,
												pref_button);

		button.name  = buttonName;
		button.layer = gameObject.layer;

		Vector3 position = Vector3.zero;

		//TODO refazer essa parte
		uiGrid.cellWidth  = uiGrid.cellHeight = 50;
		uiGrid.maxPerLine = maxPerLine;
		uiGrid.sorted = true;

		if(IsVerticalQueue)
		{
			//TODO
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
		}
		else
		{
			uiGrid.arrangement = UIGrid.Arrangement.Horizontal;
			//FIXME
			//position = new Vector3 (rootPosition.x + ((index - 1) * texture.width), rootPosition.y, -5);
		}

		button.transform.localPosition = position;

		if(!string.IsNullOrEmpty(textureName))
		{
			Transform trnsForeground = button.transform.FindChild("Foreground");
			ChangeButtonForegroundTexture(trnsForeground, textureName);
		}

		DefaultCallbackButton dcb = button.AddComponent<DefaultCallbackButton>();

		dcb.Init(ht, onClick, onPress, onDrag, onDrop);

		StartCoroutine("RepositionGrid");
	}

	public void DequeueMessageInfo()
	{
		if(uiGrid.transform.childCount == 0)
			return;

		--nQueueItems;

		int childIndex = uiGrid.transform.childCount - 1;

		Destroy (uiGrid.transform.GetChild(childIndex).gameObject);
		Invoke("RepositionGrid", 0.1f);
	}

	public bool CheckQueuedButtonIsFirst(string buttonName)
	{
		Transform trnsButton = uiGrid.transform.FindChild(buttonName);

		if(trnsButton == null)
		{
			return false;
		}

		Debug.Log("trnsButton.localPosition: " + trnsButton.localPosition);
		return trnsButton.localPosition == Vector3.zero;
	}

	public void RemoveMessageInfo(string buttonName)
	{
		Transform trnsButton = uiGrid.transform.FindChild(buttonName);

		if(trnsButton != null)
		{
			--nQueueItems;

			Destroy (trnsButton.gameObject);
			Invoke("RepositionGrid", 0.1f);
		}
	}

	public void Clear()
	{
		nQueueItems = 0;

		foreach (Transform child in uiGrid.transform)
		{
			Destroy (child.gameObject);
		}

		Invoke("RepositionGrid", 0.1f);
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

	private void RepositionGrid()
	{
		if(uiGrid.transform.childCount != nQueueItems)
			Invoke("RepositionGrid", 0.1f);
		else
			uiGrid.repositionNow = true;
	}
}
