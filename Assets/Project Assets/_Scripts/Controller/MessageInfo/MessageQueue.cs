using UnityEngine;
using System.Collections;

public abstract class MessageQueue : MonoBehaviour
{
	public string QueueName;
	public Vector2 RootPosition;
	public bool IsVerticalQueue;
	public int MaxPerLine;
	public int MaxItems;
	public Vector2 CellSize;

	protected GameObject Pref_button;
	protected UIGrid uiGrid;

	protected float nQueueItems;

	public void AddMessageInfo( string buttonName,
								Hashtable ht,
								DefaultCallbackButton.OnClickDelegate onClick = null,
								DefaultCallbackButton.OnPressDelegate onPress = null,
								DefaultCallbackButton.OnDragDelegate onDrag = null,
								DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		++nQueueItems;

		GameObject button = NGUITools.AddChild (uiGrid.gameObject,
												Pref_button);

		button.name  = buttonName;
		button.layer = gameObject.layer;

		//button.transform.localPosition = Vector3.zero;

		if(ht.ContainsKey("textureName"))
		{
			Transform trnsForeground = button.transform.FindChild("Foreground");
			ChangeButtonForegroundTexture(trnsForeground, (string)ht["textureName"]);
		}

		if(ht.ContainsKey("message"))
		{
			Transform trnsLabel = button.transform.FindChild("Label");
			trnsLabel.GetComponent<UILabel>().text = (string)ht["message"];
		}

		DefaultCallbackButton dcb = button.AddComponent<DefaultCallbackButton>();

		dcb.Init(ht, onClick, onPress, onDrag, onDrop);

		Invoke("RepositionGrid", 0.1f);
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

	public bool IsEmpty()
	{
		return (uiGrid.transform.childCount == 0);
	}

	public void Clear()
	{
		nQueueItems = 0;

		foreach (Transform child in uiGrid.transform)
		{
			Destroy (child.gameObject);
		}
	}

	protected void ChangeButtonForegroundTexture(Transform trnsForeground, string textureName)
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

	protected void RepositionGrid()
	{
		if(uiGrid.name == "Unit Group")
			Debug.Log("uiGrid.transform.childCount: " + uiGrid.transform.childCount);

		if(uiGrid.transform.childCount != nQueueItems)
			Invoke("RepositionGrid", 0.1f);
		else
		{
			uiGrid.repositionNow = true;
			uiGrid.Reposition();
		}
	}
}
