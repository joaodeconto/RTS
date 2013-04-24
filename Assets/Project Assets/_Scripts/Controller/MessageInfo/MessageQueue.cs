using UnityEngine;
using System.Collections;

public abstract class MessageQueue : MonoBehaviour
{
	public string QueueName { get; protected set; }
	public Vector2 RootPosition { get; protected set; }

	private Vector2 _padding;
	public Vector2 Padding
	{
		get
		{
			return _padding;
		}
		protected set
		{
			Vector2 oldPadding = _padding;
			_padding = value;
			this.uiGrid.cellWidth  = this.uiGrid.cellWidth  + (_padding.x - oldPadding.x);
			this.uiGrid.cellHeight = this.uiGrid.cellHeight + (_padding.y - oldPadding.y);
		}
	}

	public Vector2 CellSize
	{
		get {
			return new Vector2(uiGrid.cellWidth - _padding.x, uiGrid.cellHeight - _padding.y);
		}
		set {
			uiGrid.cellWidth  = value.x + _padding.x;
			uiGrid.cellHeight = value.y + _padding.y;
		}
	}

	public bool IsVerticalQueue
	{
		get
		{
			return (uiGrid.arrangement == UIGrid.Arrangement.Vertical);
		}
		protected set
		{
			this.uiGrid.arrangement = (value) ?
										UIGrid.Arrangement.Vertical :
										UIGrid.Arrangement.Horizontal;
		}
	}

	public int MaxPerLine
	{
		get { return uiGrid.maxPerLine; }
		protected set { uiGrid.maxPerLine = value; }
	}
	public int MaxItems { get; protected set; }

	public float LabelSize { get; protected set; }

	protected GameObject Pref_button;
	protected UIGrid uiGrid;

	protected float nQueueItems;

	public virtual void AddMessageInfo( string buttonName,
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
		button.transform.localPosition = Vector3.up * 100000;//Coloca em um lugar em distante para somente aparecer no reposition grid
		button.transform.FindChild("Background").localScale = new Vector3(CellSize.x, CellSize.y, 1);
		button.transform.FindChild("Foreground").localScale = new Vector3(CellSize.x, CellSize.y, 1);

		//button.transform.localPosition = Vector3.zero;

		PersonalizedCallbackButton pcb = button.AddComponent<PersonalizedCallbackButton>();

		pcb.Init(ht, onClick, onPress, onDrag, onDrop);

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
		return (nQueueItems == 0);
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
