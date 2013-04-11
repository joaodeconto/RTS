using UnityEngine;
using System.Collections;

using Visiorama;

public abstract class MessageQueue : MonoBehaviour
{
	private string NOT_IN_USE_STRING = "ZZZZ";

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

	protected int nQueueItems;

	//GameObject itemBag;

	PrefabCache prefabCache;

	public virtual MessageQueue Init(GameObject pref_button,
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
		prefabCache        = ComponentGetter.Get<PrefabCache>();

		this.Pref_button = pref_button;
		this.uiGrid      = uiGrid;

		this.QueueName   = queueName;

		this.RootPosition    = rootPosition;
		this.IsVerticalQueue = isVerticalQueue;
		this.MaxPerLine      = maxPerLine;
		this.MaxItems        = maxItems;
		this.CellSize        = cellSize;
		this.Padding         = padding;
		this.LabelSize       = labelSize;

		this.uiGrid.sorted       = true;
		this.uiGrid.hideInactive = false;

		//itemBag = new GameObject ("itemBag");
		//itemBag.transform.parent = uiGrid.transform;

		//for (int i = 0; i != maxItems; ++i)
		//{
			//GameObject button = NGUITools.AddChild (itemBag,
													//Pref_button);
			//button.name = NOT_IN_USE_STRING;

			//button.layer = gameObject.layer;
			//button.transform.localPosition = Vector3.up * 100000;//Coloca em um lugar distante para somente aparecer no reposition grid
			//button.transform.FindChild("Background").localScale = new Vector3(CellSize.x, CellSize.y, 1);
			//button.transform.FindChild("Foreground").localScale = new Vector3(CellSize.x, CellSize.y, 1);

			////button.transform.localPosition = Vector3.zero;

			//PersonalizedCallbackButton pcb = button.AddComponent<PersonalizedCallbackButton>();
			//pcb.Init ();//.Hide ();
		//}

		return this;
	}

	public virtual void AddMessageInfo( string buttonName,
										Hashtable ht,
										DefaultCallbackButton.OnClickDelegate onClick = null,
										DefaultCallbackButton.OnPressDelegate onPress = null,
										DefaultCallbackButton.OnDragDelegate onDrag = null,
										DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		++nQueueItems;

		if (nQueueItems == MaxItems)
		{
			//TODO Agrupar unidades
			Debug.LogError ("adicionado mais itens do que é possível suportar");
			return;
		}

		GameObject button = prefabCache.Get (uiGrid.transform, "button");

		UIPanel p = uiGrid.transform.parent.parent.GetComponent<UIPanel>();

		foreach (UIWidget w in button.GetComponentsInChildren <UIWidget>())
		{
			w.panel = p;
		}
			//button = NGUITools.AddChild(trnsOptionsMenu.gameObject,
										//pref_button);

		//itemBag.transform.FindChild (NOT_IN_USE_STRING).gameObject;
		//button.transform.parent = uiGrid.transform;

		button.layer = gameObject.layer;
		button.transform.localScale = Vector3.one;
		button.transform.localPosition = Vector3.up * 100000;//Coloca em um lugar distante para somente aparecer no reposition grid

		button.transform.FindChild("Background").localScale = new Vector3(CellSize.x, CellSize.y, 1);
		button.transform.FindChild("Foreground").localScale = new Vector3(CellSize.x, CellSize.y, 1);

		button.name = buttonName;

		//if (button.GetComponent<PersonalizedCallbackButton>() == null)
		//{
			//button.AddComponent<PersonalizedCallbackButton>();
		//}

		PersonalizedCallbackButton pcb = button.AddComponent<PersonalizedCallbackButton>();
		//PersonalizedCallbackButton pcb = button.GetComponent<PersonalizedCallbackButton>();

		pcb.Init ().Show (ht, onClick, onPress, onDrag, onDrop);

		Invoke("RepositionGrid", 0.1f);
	}

	public void DequeueMessageInfo()
	{
		if(uiGrid.transform.childCount == 0)
			return;

		--nQueueItems;

		int childIndex = nQueueItems;

		Transform trnsButton = uiGrid.transform.GetChild(childIndex);

		//trnsButton.parent = itemBag.transform;

		//trnsButton.GetComponent <PersonalizedCallbackButton> ().Hide ();
		//trnsButton.name = NOT_IN_USE_STRING;
		//int childIndex = uiGrid.transform.childCount - 1;
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
		return (trnsButton.localPosition == Vector3.zero);
	}

	public void RemoveMessageInfo(string buttonName)
	{
		Transform trnsButton = uiGrid.transform.FindChild(buttonName);

		if(trnsButton != null)
		{
			--nQueueItems;

			trnsButton.gameObject.GetComponent <PersonalizedCallbackButton> ();//.Hide ();
			trnsButton.name = NOT_IN_USE_STRING;

			//Destroy (trnsButton.gameObject);
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
			//child.gameObject.GetComponent <PersonalizedCallbackButton> ();//.Hide ();
			//child.name = NOT_IN_USE_STRING;
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
		//if(uiGrid.name == "Unit Group")
			//Debug.Log("uiGrid.transform.childCount: " + uiGrid.transform.childCount);

		//int childCount = 0;

		//foreach (Transform child in uiGrid.transform)
		//{
			//if (child.name != NOT_IN_USE_STRING)
				//++childCount;
		//}

		if(uiGrid.transform.childCount != nQueueItems)
		//if(childCount != nQueueItems)
			Invoke("RepositionGrid", 0.1f);
		else
		{
			uiGrid.repositionNow = true;
			uiGrid.Reposition();

			//StartCoroutine (() =>
							//{
								//foreach (Transform child in uiGrid.transform)
								//{
									//if (child.name == NOT_IN_USE_STRING)
										//child.localPosition = Vector3.up * 10000;
								//}
							//});
		}
	}
}
