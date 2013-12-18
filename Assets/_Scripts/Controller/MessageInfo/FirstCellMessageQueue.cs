using UnityEngine;
using System.Collections;

public class FirstCellMessageQueue : MessageQueue
{
	public Vector2 firstCellrootPosition;
	public Vector2 firstCellSize;
	public UIGrid  uiGridFirst;
	public Vector2 FirstCellRootPosition { get; protected set; }
	//	public float   nQueueItemFirst;

	public Vector2 FirstCellSize
	{
		get {
			return new Vector2(uiGridFirst.cellWidth, uiGridFirst.cellHeight);
		}
		set {
			uiGridFirst.cellWidth  = value.x;
			uiGridFirst.cellHeight = value.y;
		}
	}


	public FirstCellMessageQueue Init (  GameObject pref_button,
										 UIGrid uiGrid,
	                                     UIGrid uiGridFirst,
	                                     string queueName,
										 Vector2 rootPosition,
										 Vector2 cellSize,
										 Vector2 padding,
										 float labelSize,
	                                     Vector2 firstCellRootPosition,
	                                     Vector2 firstCellSize,	                                     					 
										 int maxPerLine,
										 int maxItems)
	{
		this.Pref_button = pref_button;
		this.uiGrid = uiGrid;
		this.uiGridFirst = uiGridFirst;
		this.QueueName   = queueName;
		this.RootPosition    = RootPosition;
		this.IsVerticalQueue = IsVerticalQueue;
		this.MaxPerLine      = maxPerLine;
		this.MaxItems        = maxItems;
		this.CellSize        = cellSize;
		this.Padding         = padding;
		this.LabelSize       = labelSize;
		this.FirstCellSize   = firstCellSize;
		this.FirstCellRootPosition = firstCellRootPosition;
		this.uiGrid.maxPerLine = maxPerLine;
		this.uiGridFirst.sorted       = true;
		this.uiGridFirst.hideInactive = false;
		this.uiGrid.sorted       = true;
		this.uiGrid.hideInactive = false;

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
	//Verificar presença de btns
    //Sim - adicionar ma uiGrid else uiGridFirst
	 
		if (uiGridFirst.transform.childCount == 0)
		{

				++nQueueItems;
			
			//		Debug.Log ("Sem botoes");
			//		return;
			

				GameObject button = NGUITools.AddChild (uiGridFirst.gameObject,
				                                        Pref_button);
				
				button.name  = buttonName;
				button.layer = gameObject.layer;
				button.transform.localPosition = Vector3.up * 100000;
			    button.transform.FindChild("Foreground").localScale = new Vector3(firstCellSize.x, firstCellSize.y, 1);
			   
	     		
				//button.transform.localPosition = Vector3.zero;
				
				PersonalizedCallbackButton pcb = button.AddComponent<PersonalizedCallbackButton>();
				
				pcb.Init(ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);
				


		}



		else

		{   
			++nQueueItems;

			GameObject button = NGUITools.AddChild (uiGrid.gameObject,
			                                        Pref_button);
			
			button.name  = buttonName;
			button.layer = gameObject.layer;
			button.transform.localPosition = Vector3.up * 100000;//Coloca em um lugar em distante para somente aparecer no reposition grid
			button.transform.FindChild("Foreground").localScale = new Vector3(CellSize.x, CellSize.y, 1);
			
			//button.transform.localPosition = Vector3.zero;
			
			PersonalizedCallbackButton pcb = button.AddComponent<PersonalizedCallbackButton>();
			
			pcb.Init(ht, onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);

		}


		Invoke("RepositionGrid", 0.1f);
	}	

	public void DequeueMessageInfo()
	{
		if(uiGridFirst.transform.childCount == 0)
			return;
		
		--nQueueItems;
		
		int childIndex = uiGridFirst.transform.childCount - 1;
		
		Destroy (uiGridFirst.transform.GetChild(childIndex).gameObject);
		Invoke("RepositionGrid", 0.1f);
	}
	
	public bool CheckQueuedButtonIsFirst(string buttonName)
	{
		Transform trnsButton = uiGridFirst.transform.FindChild(buttonName);
		
		if(trnsButton == null)
		{
			return false;
		}
		
		Debug.Log("trnsButton.localPosition: " + trnsButton.localPosition);
		return trnsButton.localPosition == Vector3.zero;
	}
	
	public void RemoveMessageInfo(string buttonName)
	{
		Transform trnsButton = uiGridFirst.transform.FindChild(buttonName);
		
		if(trnsButton != null)
		{
			--nQueueItems;
			
			Destroy (trnsButton.gameObject);
			Invoke("RepositionGrid", 0.1f);
		}
	}





}
