using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using PathologicalGames;

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
	public UIGrid uiGrid;

	protected int nQueueItems;

	public virtual void AddMessageInfo( string buttonName,
										Hashtable ht,
										DefaultCallbackButton.OnClickDelegate onClick = null,
										DefaultCallbackButton.OnPressDelegate onPress = null,
										DefaultCallbackButton.OnSliderChangeDelegate onSliderChange = null,
										DefaultCallbackButton.OnActivateDelegate onActivate = null,
										DefaultCallbackButton.OnRepeatClickDelegate onRepeatClick = null,
										DefaultCallbackButton.OnDragDelegate onDrag = null,
										DefaultCallbackButton.OnDropDelegate onDrop = null)
//										DefaultCallbackButton.OnClickDelegate onGroupClick = null)
	{
		++nQueueItems;
				
//		GameObject button = NGUITools.AddChild (uiGrid.gameObject, Pref_button);
		Transform button = PoolManager.Pools["Buttons"].Spawn(Pref_button, uiGrid.transform);		
		button.localScale = Vector3.one;
		PersonalizedCallbackButton pcb = button.GetComponent<PersonalizedCallbackButton>();		
		if ( pcb == null )
		{
			pcb = button.gameObject.AddComponent<PersonalizedCallbackButton>();
			pcb.Init(ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);
		}
		
		if (nQueueItems > MaxItems)
		{
			// Refazendo o calculo
			if (nQueueItems - 1 == MaxItems) ChangeToGroupMessageInfo ();			
			buttonName = RegexClone (buttonName);			
			if (CheckExistMessageInfo (buttonName)) return;			
			button.name  = buttonName;
			button.transform.localPosition = Vector3.up * 10000;

			pcb.ChangeParams(ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);
		}
		else
		{			
			button.name  = buttonName;
			button.transform.localPosition = Vector3.up * 100000;
			pcb.ChangeParams(ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);
		}
		
		Invoke("RepositionGrid", 0.1f);
	}

	public void DequeueMessageInfo()
	{
		if(uiGrid.transform.childCount == 0)	return;
		--nQueueItems;
		int childIndex = uiGrid.transform.childCount - 1;
		Transform goMessageInfo = uiGrid.transform.GetChild(childIndex);
		HealthObserverButton hbo = goMessageInfo.GetComponent<HealthObserverButton> ();
		if (hbo != null) hbo.StopToObserve ();
		DespawnBtn(goMessageInfo);		
		Invoke("RepositionGrid", 0.1f);
	}

	public bool CheckQueuedButtonIsFirst(string buttonName)
	{
		Transform trnsButton = uiGrid.transform.FindChild(buttonName);

		if(trnsButton == null)
		{
			return false;
		}
		return trnsButton.localPosition == Vector3.zero;
	}

	public void RemoveMessageInfo(string buttonName)
	{
		Transform trnsButton = uiGrid.transform.FindChild(buttonName);

		if(trnsButton != null)
		{
			--nQueueItems;
			DespawnBtn(trnsButton);	
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
			DespawnBtn(child);	
		}
	}

	protected void ChangeButtonForegroundTexture(Transform trnsForeground, string textureName)
	{
		if(trnsForeground == null || trnsForeground.GetComponent<UISprite>() == null)
		{
			Debug.LogError("Eh necessario que tenha o objeto \"Foreground\" com um sliced sprite dentro");
			Debug.Break();
		}

		UISprite sprite = trnsForeground.GetComponent<UISprite>();
		sprite.spriteName = textureName;
		sprite.height = Mathf.CeilToInt (CellSize.y);
		sprite.width = Mathf.CeilToInt (CellSize.x);
//		sprite.MakePixelPerfect();

	} 

	protected void RepositionGrid()
	{
//		if(uiGrid.transform.childCount != nQueueItems)
//		{
//			Invoke("RepositionGrid", 0.1f);
//			Debug.Log("reposition nQueue != childcount");
//		}
//		else
//		{
			uiGrid.repositionNow = true;
			uiGrid.Reposition();			
//			Debug.Log("reposition else");
//		}
	}
	
	protected void ChangeToGroupMessageInfo ()
	{
		for (int i = 0; i != uiGrid.transform.childCount; ++i)
		{
			uiGrid.transform.GetChild (i).name = RegexClone (uiGrid.transform.GetChild (i).name);
			for (int k = 0; k != i; ++k)
			{
				if (uiGrid.transform.GetChild (k).name.Equals (uiGrid.transform.GetChild (i).name))
				{
					--nQueueItems;
//					Destroy (uiGrid.transform.GetChild (i));
					Transform child = uiGrid.transform.GetChild(i);				
					DespawnBtn(child);					
					--i;
					break;
				}
			}
		}
	}

	public void DespawnBtn(Transform btnTrns)
	{
		btnTrns.parent = PoolManager.Pools["Buttons"].group;
		btnTrns.name = "alreadyUsed" + (int)Time.time;
		PoolManager.Pools["Buttons"].Despawn (btnTrns);		
	}
	
	protected bool CheckExistMessageInfo (string name)
	{
		for (int i = 0; i != uiGrid.transform.childCount; ++i)
		{
			if (uiGrid.transform.GetChild (i).name.Equals (name))
			{
				return true;
			}
		}
		
		return false;
	}

	protected string RegexClone (string name)
	{
		name = Regex.Split (name, "(Clone)")[0];
		name = name.Split ('(')[0];
		return name;
	}
}