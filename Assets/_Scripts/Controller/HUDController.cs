using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;
using Visiorama.Utils;

public class HUDController : MonoBehaviour, IDeathObserver
{
	private string PERSIST_STRING = "###_";

	[System.Serializable]

	public class GridDefinition
	{
		public string name;
		public Vector3 rootPosition;
		public float xPadding;
		public float yPadding;
		public float cellSizeX;
		public float cellSizeY;

		public Vector3 GetGridPosition(int xIndex, int yIndex)
		{
			Vector2 vec = Math.GetGridPosition(xIndex,   yIndex,
											   xPadding, yPadding,
											   cellSizeX,cellSizeY);
			return rootPosition + new Vector3(vec.x, vec.y, 0);
		}
	}

	public enum Feedbacks
	{
		Move,
		Attack,
		Self
	}

	[System.Serializable]
	public class ButtonStatus
	{
		public string buttonName;
		public Vector3 position;
		public Hashtable ht;
		public string textureName;
		public DefaultCallbackButton.OnClickDelegate onClick;
		public DefaultCallbackButton.OnPressDelegate onPress;
		public DefaultCallbackButton.OnSliderChangeDelegate onSliderChange;
		public DefaultCallbackButton.OnActivateDelegate onActivate;
		public DefaultCallbackButton.OnRepeatClickDelegate onRepeatClick;
		public DefaultCallbackButton.OnDragDelegate onDrag;
		public DefaultCallbackButton.OnDropDelegate onDrop;
		public bool persistent;
	}

	public GameObject pref_healthBar;
	public GameObject pref_SubstanceHealthBar;
	public GameObject pref_SubstanceResourceBar;
	public GameObject pref_selectedObject;

	public UIRoot uiRoot;
	public Transform mainTranformSelectedObjects;
	public Transform trnsOptionsMenu;
	public Transform trnsPanelInfoBox;
	public Transform trnsPanelUnitStats;

	public GameObject pref_button;
	public Vector3 offesetFeedback;
	public GameObject pref_moveFeedback;
	public GameObject pref_selfFeedback;
	public GameObject pref_attackFeedback;

	public UISlider[] sliders;

	public UISlider GetSlider(string name)
	{
		foreach(UISlider slider in sliders)
		{
			if(slider.name.ToLower().Equals(name.ToLower()))
			{
				return slider;
			}
		}
		return null;
	}

	public GridDefinition[] grids;

	public GridDefinition GetGrid(string name)
	{
		foreach(GridDefinition gd in grids)
		{
			if(gd.name.Equals(name))
				return gd;
		}
		return null;
	}

	private List<UIGrid> gridsToReposition = new List<UIGrid>();

	private GameObject oldFeedback;

	private TouchController touchController;
	private MessageInfoManager messageInfoManager;
	private PrefabCache prefabCache;

	private Stack<ButtonStatus> stackButtonToCreate;

	private bool _isDestroying;
	private bool IsDestroying {
		get	{
			if(_isDestroying)
			{
				bool hasChild = false;
				foreach(Transform c in trnsOptionsMenu)
				{
					if (!c.gameObject.name.Contains(PERSIST_STRING))
					{
						hasChild = true;
						break;
					}
				}
				_isDestroying = hasChild;
			}
			return _isDestroying;
		}
		set	{
			_isDestroying = value;
		}
	}
	
//	private Transform infoStats;
//	private Transform infoQuali;
//	private Transform infoIcon;
//	
//	private UILabel attackLabel;
//	private UILabel hpLabel;
//	private UILabel speedLabel;
//	private UILabel timeLabel;
//	private UILabel unitsLabel;
//	
//	private UILabel nameLabel;
//	private HealthBar currentHp;
//	
//	private UISprite spriteFactory;
//	private UISprite spriteUnit;
	
//	private string cGODisplayedOnInfoBox;

	public void Init()
	{
		messageInfoManager = ComponentGetter.Get<MessageInfoManager>();
		touchController    = ComponentGetter.Get<TouchController>();
		//prefabCache        = ComponentGetter.Get<PrefabCache>();

		stackButtonToCreate = new Stack<ButtonStatus>();

		IsDestroying = false;
		
//		infoStats = trnsPanelInfoBox.FindChild ("Info Stats");
//		infoQuali = trnsPanelInfoBox.FindChild ("Info Qualities");
//		infoIcon  = trnsPanelInfoBox.FindChild ("Info Icon");
//		
//		attackLabel = infoStats.FindChild ("attack-label").GetComponent<UILabel> ();
//		hpLabel	    = infoStats.FindChild ("hp-label").GetComponent<UILabel> ();
//		speedLabel  = infoStats.FindChild ("speed-label").GetComponent<UILabel> ();
//		unitsLabel  = infoStats.FindChild ("units-label").GetComponent<UILabel> ();
//		timeLabel   = infoStats.FindChild ("time-label").GetComponent<UILabel> ();
//		
//		nameLabel = infoQuali.FindChild ("name-label").GetComponent<UILabel> ();
//		currentHp = infoQuali.FindChild ("CurrentHP").gameObject.GetComponent <HealthBar> ();
//		
//		spriteFactory = infoIcon.FindChild ("sprite-unit").GetComponent<UISprite> ();
//		spriteUnit	  = infoIcon.FindChild ("sprite-unit").GetComponent<UISprite> ();
	}

	public HealthBar CreateHealthBar (IStats target, int maxHealth, string referenceChild)
	{
		GameObject child = NGUITools.AddChild(trnsPanelUnitStats.gameObject, pref_healthBar);

		child.GetComponent<UISlider> ().foregroundWidget.width = Mathf.CeilToInt (maxHealth * 0.6f);
		child.GetComponent<UISlider> ().foregroundWidget.height = 8;

		child.AddComponent<UIFollowTarget>().target      = target.transform.FindChild (referenceChild).transform;
		child.GetComponent<UIFollowTarget>().mGameCamera = touchController.mainCamera;
		child.GetComponent<UIFollowTarget>().mUICamera   = uiRoot.transform.FindChild ("CameraHUD").camera;

		HealthBar healthBar = child.GetComponent<HealthBar> ();

		healthBar.SetTarget (target);

		return healthBar;
	}

	public SubstanceResourceBar CreateSubstanceResourceBar (IStats target, float size, float maxResource)
	{
		GameObject selectObj = Instantiate (pref_SubstanceResourceBar, target.transform.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;
		
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target.transform;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.1f;
		
		selectObj.transform.parent = mainTranformSelectedObjects;
		
		SubstanceResourceBar resourceBar = selectObj.GetComponent<SubstanceResourceBar> ();
			      	
		//healthBar.SetTarget (target, target.team);
		
		return resourceBar;
	}
	
	public SubstanceHealthBar CreateSubstanceHealthBar (IStats target, float size, int maxHealth, string referenceChild)
	{
		GameObject selectObj = Instantiate (pref_SubstanceHealthBar, target.transform.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;

		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target.transform;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.1f;
		
		selectObj.transform.parent = mainTranformSelectedObjects;
		
		SubstanceHealthBar subHealthBar = selectObj.GetComponent<SubstanceHealthBar> ();
		
		subHealthBar.SetTarget (target, target.team);

				
		return subHealthBar;
	}

	public void CreateSelected (Transform target, float size, Color color)
	{
		GameObject selectObj = Instantiate (pref_selectedObject, target.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
			
		foreach (ParticleSystem ps in selectObj.GetComponentsInChildren<ParticleSystem>())
		{
			ps.startSize = size * 2f;
			ps.renderer.material.SetColor ("_TintColor", color);
			ps.startColor = color;
		}
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.1f;

		selectObj.renderer.material.SetColor ("_TintColor", color);

		selectObj.transform.parent = mainTranformSelectedObjects;
	}

	public bool HasSelected (Transform target)
	{
		bool hasSelected = false;
		
		foreach (Transform child in mainTranformSelectedObjects)
		{
			if (child.GetComponent<ReferenceTransform>().referenceObject == target)
			{
				hasSelected = true;
				break;
			}
		}
		
		if (!hasSelected)
		{
			foreach (Transform child in trnsPanelUnitStats)
			{
				if (child.GetComponent<HealthBar>())
				{
					if (child.GetComponent<HealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
					{
						hasSelected = true;
                        break;
					}
				}
				
				if (child.GetComponent<SubstanceHealthBar>())
	            {
	                if (child.GetComponent<SubstanceHealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
					{
						hasSelected = true;
						break;
                    }
	            }
	        }
		}
		
		return hasSelected;
	}

	public void DestroySelected (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			if (child.GetComponent<ReferenceTransform>().referenceObject == target)
			{
				DestroyObject (child.gameObject);
			}
		}

		foreach (Transform child in trnsPanelUnitStats)
		{
			if (child.GetComponent<HealthBar>())
			{
				if (child.GetComponent<HealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
				{
					Destroy (child.gameObject);
				}
			}
			
//			if (child.GetComponent<SubstanceHealthBar>())
//			{
//				if (child.GetComponent<SubstanceHealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
//				{
//					Destroy (child.gameObject);
//				}
//			}
		}
	}

	public void CreateEnqueuedButtonInInspector(string buttonName,
												string queueName,
												Hashtable ht,
												string textureName = "",
												DefaultCallbackButton.OnClickDelegate onClick = null,
												DefaultCallbackButton.OnPressDelegate onPress = null,
												DefaultCallbackButton.OnSliderChangeDelegate onSliderChange = null,
												DefaultCallbackButton.OnActivateDelegate onActivate = null,
												DefaultCallbackButton.OnRepeatClickDelegate onRepeatClick = null,
												DefaultCallbackButton.OnDragDelegate onDrag = null,
												DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		if (ht == null)
			ht = new Hashtable();

		if(!string.IsNullOrEmpty(textureName))
			ht["textureName"] = textureName;

		MessageQueue mq = messageInfoManager.GetQueue(queueName);
		mq.AddMessageInfo ( buttonName, ht,
							onClick, onPress, onSliderChange, onActivate, onRepeatClick, onDrag, onDrop);
	}

	public void RemoveEnqueuedButtonInInspector(string buttonName, string queueName)
	{
		MessageQueue mq = messageInfoManager.GetQueue(queueName);
	
		mq.RemoveMessageInfo(buttonName);
	}

	public void DequeueButtonInInspector(string queueName)
	{
		MessageQueue mq = messageInfoManager.GetQueue(queueName);

		mq.DequeueMessageInfo();
	}

	public bool CheckQueuedButtonIsFirst(string buttonName, string queueName)
	{
		MessageQueue mq = messageInfoManager.GetQueue(queueName);
		
		return mq.CheckQueuedButtonIsFirst(buttonName);
	}

	public void CreateButtonInInspector(string buttonName,
										Vector3 position,
										Hashtable ht,
										string textureName = "",
										DefaultCallbackButton.OnClickDelegate onClick = null,
                                        DefaultCallbackButton.OnPressDelegate onPress = null,
                                        DefaultCallbackButton.OnDragDelegate onDrag = null,
                                        DefaultCallbackButton.OnDropDelegate onDrop = null,
										bool persistent = false)
	{
		CreateOrChangeButtonInInspector(buttonName, position, ht,
										textureName,
										onClick, onPress, onDrag, onDrop,
										persistent);
	}

	public void CreateOrChangeButtonInInspector(string buttonName,
												Vector3 position,
												Hashtable ht,
												string textureName = "",
												DefaultCallbackButton.OnClickDelegate onClick = null,
												DefaultCallbackButton.OnPressDelegate onPress = null,
												DefaultCallbackButton.OnDragDelegate onDrag = null,
												DefaultCallbackButton.OnDropDelegate onDrop = null,
												bool persistent = false)
	{
		ButtonStatus bs = new ButtonStatus();

		bs.buttonName  = buttonName;
		bs.position    = position;
		bs.ht          = ht;
		bs.textureName = textureName;
		bs.onClick     = onClick;
		bs.onPress     = onPress;
		bs.onDrag      = onDrag;
		bs.onDrop      = onDrop;
		bs.persistent  = persistent;

		stackButtonToCreate.Push(bs);

		StartCoroutine("CreateButton");
	}

	IEnumerator CreateButton()
	{
		while (IsDestroying)
		{
			yield return new WaitForSeconds(0.001f);
		}

		ButtonStatus bs = stackButtonToCreate.Pop();
		string buttonName = bs.persistent ?
								PERSIST_STRING + bs.buttonName :
								bs.buttonName;

		if(!string.IsNullOrEmpty(bs.textureName))
			bs.ht["textureName"] = bs.textureName;

		Transform buttonParent = null;
		if (bs.ht.ContainsKey ("parent"))
		{
			buttonParent = (Transform)bs.ht["parent"];
		}
		else
		{
			buttonParent = trnsOptionsMenu;
		}

		GameObject button = GetButtonInHUD (buttonName, buttonParent);

		button.name = buttonName;
		button.transform.localPosition = bs.position;

		PersonalizedCallbackButton pcb = button.GetComponent<PersonalizedCallbackButton>();

		if ( pcb == null )
		{
			pcb = button.AddComponent<PersonalizedCallbackButton>();
			pcb.Init(bs.ht, bs.onClick, bs.onPress, bs.onSliderChange, bs.onActivate, bs.onRepeatClick, bs.onDrag, bs.onDrop);
		}
		else
			pcb.ChangeParams(bs.ht, bs.onClick, bs.onPress, bs.onSliderChange, bs.onActivate, bs.onRepeatClick, bs.onDrag, bs.onDrop);
	}

	GameObject GetButtonInHUD (string buttonName, Transform parentButton)
	{
		GameObject button = null;
		
		Transform trns = parentButton.Find(buttonName);
		if (trns != null)
			button = trns.gameObject;
		else
			//button = prefabCache.Get(trnsOptionsMenu, "Button");
			button = NGUITools.AddChild(parentButton.gameObject,
                                        pref_button);
                                        
		return button;
	}

	public void RemoveButtonInInspector(string buttonName, Transform buttonParent = null)
	{
		Transform trns = (buttonParent != null) ? buttonParent : trnsOptionsMenu;
	
		foreach (Transform child in trns)
		{
			if (child.gameObject.name.Equals(buttonName) ||
				child.gameObject.name.Equals(PERSIST_STRING + buttonName))
			{
				Destroy (child.gameObject);
				break;
			}
		}
	}

	public void DestroyOptionsBtns ()

	{
		IsDestroying = true;

		foreach (Transform child in trnsOptionsMenu)
		{
				Destroy (child.gameObject);
		}


	}

	public void DestroyInspector (string type)
	{
		IsDestroying = true;

		if (type.ToLower ().Equals ("factory"))
			messageInfoManager.ClearQueue(FactoryBase.FactoryQueueName);
		else if (type.ToLower ().Equals ("unit"))
			messageInfoManager.ClearQueue(Unit.UnitGroupQueueName);
		else
		{
			messageInfoManager.ClearQueue(FactoryBase.FactoryQueueName);
			messageInfoManager.ClearQueue(Unit.UnitGroupQueueName);
		}
	}

	public void CreateFeedback (Feedbacks feedback, Transform transform, float size, Color color)
	{
		CreateFeedback (feedback, transform.position, size, color);
		oldFeedback.transform.parent = transform;
	}

	public void CreateFeedback (Feedbacks feedback, Vector3 position, float size, Color color)
	{
		if (oldFeedback != null) Destroy (oldFeedback);

		GameObject newFeedback   = null;
		GameObject pref_feedback = null;

		switch (feedback)
		{
			case Feedbacks.Move:
				pref_feedback = pref_moveFeedback;
				break;
			case Feedbacks.Self:
				pref_feedback = pref_selfFeedback;
				break;
			case Feedbacks.Attack:
				pref_feedback = pref_attackFeedback;
				break;
		}

		newFeedback = Instantiate (pref_feedback, position + offesetFeedback, Quaternion.identity) as GameObject;
		newFeedback.name = "Feedback";

//		newFeedback.transform.localScale = size * newFeedback.transform.localScale;
		newFeedback.transform.eulerAngles = new Vector3 (90,0,0);
		newFeedback.renderer.material.SetColor ("_TintColor", color);

		float duration = 0;

		foreach (ParticleSystem ps in newFeedback.GetComponentsInChildren<ParticleSystem>())
		{
			ps.startSize = size * ps.startSize;
			ps.renderer.material.SetColor ("_TintColor", color);
			ps.startColor = color;
			if (ps.duration > duration) duration = ps.duration;
		}

		oldFeedback = newFeedback;

		Destroy (newFeedback, duration);
	}
	
//	public void OpenInfoBoxUnit (Unit unit)
//	{
//		cGODisplayedOnInfoBox = unit.name;
//		
//		RemoveEnqueuedButtonInInspector (unit.name, Unit.UnitGroupQueueName);
//				
//		nameLabel.text = unit.category;
//				
//		spriteUnit.spriteName = unit.guiTextureName;
//
//		currentHp.SetTarget (unit);
//		
//		// Info
//				
//		attackLabel.text = (unit.AdditionalForce != 0)
//							? unit.force + "(+" + unit.AdditionalForce + ")"
//							: unit.force.ToString ();
//				
//		hpLabel.text = unit.Health.ToString ();
//		speedLabel.text = ((int)unit.speed).ToString ();
//		unitsLabel.text = unit.numberOfUnits.ToString ();
//		timeLabel.text = unit.timeToSpawn.ToString ();
//		
////		Transform goldLabel = infoStats.FindChild ("gold-label");
////		goldLabel.GetComponent<UILabel> ().text = unit.costOfResources.ToString ();
//		
//		unit.RegisterDeathObserver (this);
//		
//		//Ligar painel so por ultimo
//		trnsPanelInfoBox.gameObject.SetActive (true);
//		infoStats.gameObject.SetActive (true);
//	}

//	public void OpenInfoBoxFactory (FactoryBase factory)
//	{
//		cGODisplayedOnInfoBox = factory.name;
//		
//		nameLabel.text = factory.category;
//		
//		spriteFactory.spriteName = factory.guiTextureName;
//
//		currentHp.SetTarget (factory);
		// Info
//		Transform attackLabel = infoStats.FindChild ("attack-label");
//		attackLabel.GetComponent<UILabel> ().text = unit.AdditionalForce != 0 ?
//			unit.force + "(+" + unit.AdditionalForce + ")" :
//				unit.force.ToString ();
//		
//		Transform hpLabel = infoStats.FindChild ("hp-label");
//		hpLabel.GetComponent<UILabel> ().text = stat.Health.ToString ();
//		
//		Transform speedLabel = infoStats.FindChild ("speed-label");
//		speedLabel.GetComponent<UILabel> ().text = ((int)unit.speed).ToString ();
//		
//		Transform unitsLabel = infoStats.FindChild ("units-label");
//		unitsLabel.GetComponent<UILabel> ().text = unit.numberOfUnits.ToString ();
//		
//		Transform timeLabel = infoStats.FindChild ("time-label");
//		timeLabel.GetComponent<UILabel> ().text = unit.timeToSpawn.ToString ();
//		
//		Transform goldLabel = infoStats.FindChild ("gold-label");
//		goldLabel.GetComponent<UILabel> ().text = stat.costOfResources.ToString ();
//		factory.RegisterDeathObserver (this);
		//Ligar painel so por ultimo
//		infoStats.gameObject.SetActive (false);
//		trnsPanelInfoBox.gameObject.SetActive (true);
//	}	
//	public void CloseInfoBox ()
//	{
//		trnsPanelInfoBox.gameObject.SetActive (false);
//	}

	#region IDeathObserver implementation

	public void OnObservableDie (GameObject dyingGO)
	{
//		if (dyingGO.name.Equals (cGODisplayedOnInfoBox))
//		{
//			CloseInfoBox ();
//		}
	}

	#endregion
}