using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using Visiorama;
using Visiorama.Utils;

public class HUDController : MonoBehaviour, IDeathObserver
{
	private string PERSIST_STRING = "###_";

	#region Serializable
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
	#endregion

	#region Declares: Grid, Sliders,
	public UISlider[] sliders;
	public UIButton boostProduction;
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
	private Stack<ButtonStatus> stackButtonToCreate;
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
	#endregion

	#region Feedback Declares
	public enum Feedbacks
	{
		Move,
		Attack,
		Self,
		Invalid
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
	public Vector3 	  offesetFeedback;
	public GameObject pref_moveFeedback;
	public GameObject pref_selfFeedback;
	public GameObject pref_attackFeedback;
	public GameObject pref_invalidFeedback;

	public GameObject rocksFeedback;
	public GameObject manaFeedback;
	public GameObject houseFeedback;

	private Transform oldFeedback;
	private TouchController touchController;
	private MessageInfoManager messageInfoManager;
	private FactoryBase factoryBase;	
	public  bool isClearing = false;
	private bool _isDestroying;
	public  bool IsDestroying {
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
	#endregion

	#region Info Obj Declares;
	
	private Transform infoUnit;
	private Transform infoFactory;
	private Transform infoUpgrade;
	private Transform infoQuali;
	private Transform infoIcon;
	private Transform infoReq;	
	private UILabel attackLabel;
	private UILabel hpLabel;
	private UILabel speedLabel;
	private UILabel timeLabel;
	private UILabel unitsLabel;
	private UILabel defLabel;
	private UILabel descriptLabel;
	private UILabel stats1;
	private UILabel stats2;
	private UILabel stats1Text;
	private UILabel stats2Text;
	private UILabel reqLabel;
	private UILabel bonusAtkLabel;
	private UILabel bonusDefLabel;
	private UILabel bonusSpdLabel;
	private UILabel skillLabel;
	private UILabel nameLabel;
//	private HealthBar currentHp;	
//	private UISprite spriteFactory;
//	private UISprite spriteUnit;	
	private string cGODisplayedOnInfoBox;
	#endregion

	#region Init
	public void Init()
	{
		messageInfoManager = ComponentGetter.Get<MessageInfoManager>();
		touchController    = ComponentGetter.Get<TouchController>();
		stackButtonToCreate = new Stack<ButtonStatus>();
		IsDestroying = false;
		infoUpgrade = trnsPanelInfoBox.FindChild ("Info Upgrade");
		infoFactory = trnsPanelInfoBox.FindChild ("Info Factory");
		infoUnit 	= trnsPanelInfoBox.FindChild ("Info Unit");
		infoQuali 	= trnsPanelInfoBox.FindChild ("Info Qualities");
		infoIcon  	= trnsPanelInfoBox.FindChild ("Info Icon");
		infoReq 	= trnsPanelInfoBox.FindChild ("Info Require");
		nameLabel 	= infoQuali.FindChild ("name-label").GetComponent<UILabel> ();
//		spriteFactory = infoIcon.FindChild ("sprite-unit").GetComponent<UISprite> ();
//		spriteUnit	  = infoIcon.FindChild ("sprite-unit").GetComponent<UISprite> ();
	}
	#endregion

	#region Feedback Creation Methods
	public SubstanceResourceBar CreateSubstanceResourceBar (IStats target, float size, float timeToCreateBar)
	{
		Transform selectObj = PoolManager.Pools["Selection"].Spawn(pref_SubstanceResourceBar, target.transform.position, Quaternion.identity);
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.inUpdate = false;
		refTransform.referenceObject = target.transform;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.offsetPosition.y = 0.1f;	
		SubstanceResourceBar resourceBar = selectObj.GetComponent<SubstanceResourceBar> ();
		resourceBar.refTarget = target;
		resourceBar.Init();	
		refTransform.Init();
		return resourceBar;
	}

	public SubstanceResourceBar CreateSubstanceConstructBar (IStats target, float size, float timeToCreateBar, bool noTimer = false)
	{
		Transform selectObj = PoolManager.Pools["Selection"].Spawn(pref_SubstanceResourceBar, target.transform.position, Quaternion.identity);
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);		
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.inUpdate = false;
		refTransform.referenceObject = target.transform;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.offsetPosition.y = 0.1f;	
		SubstanceResourceBar resourceBar = selectObj.GetComponent<SubstanceResourceBar> ();
		resourceBar.refTarget = target;
		resourceBar.noTimer = noTimer;
		resourceBar.Init();
		refTransform.Init();
		return resourceBar;
	}

		
	public SubstanceHealthBar CreateSubstanceHealthBar (IStats target, float size, int maxHealth, string referenceChild)
	{
		Transform selectObj = PoolManager.Pools["Selection"].Spawn(pref_SubstanceHealthBar, target.transform.position, Quaternion.identity);
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		bool isInUpgrade = true;
		if(target.GetComponent<FactoryBase>() != null) isInUpgrade = false;
		refTransform.inUpdate = isInUpgrade;
		refTransform.referenceObject = target.transform;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.offsetPosition.y = 0.1f;		
		SubstanceHealthBar subHealthBar = selectObj.GetComponent<SubstanceHealthBar> ();		
		subHealthBar.SetTarget (target, target.team);
		refTransform.Init();
		return subHealthBar;
	}

	public void CreateSelected (Transform target, float size, Color color)
	{
		Transform selectObj = PoolManager.Pools["Selection"].Spawn(pref_selectedObject, target.position, Quaternion.identity);
		selectObj.transform.localScale = new Vector3(size * 1f, size* 1f, size * 1f);
		selectObj.transform.localEulerAngles = new Vector3(90,0,0);
					
		foreach (ParticleSystem ps in selectObj.GetComponentsInChildren<ParticleSystem>())
		{
			ps.startSize = Mathf.Clamp (size * 1.6f, 2, 10);
			ps.renderer.material.SetColor ("_TintColor", color);
			ps.startColor = color;
		}

		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		bool isInUpgrade = true;
		if(target.GetComponent<FactoryBase>() != null) isInUpgrade = false;
		refTransform.inUpdate = isInUpgrade;
		refTransform.referenceObject = target;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.offsetPosition.y = 0.1f;	
		selectObj.renderer.material.SetColor ("_TintColor", color);
		refTransform.Init();
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
	

	#endregion

	#region Selected
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
		
//		if (!hasSelected)
//		{
//			foreach (Transform child in trnsPanelUnitStats)
//			{
//				if (child.GetComponent<HealthBar>())
//				{
//					if (child.GetComponent<HealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
//					{
//						hasSelected = true;
//                        break;
//					}
//				}	
//	        }
//		}
		
		return hasSelected;
	}

	public void DestroySelected (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			ReferenceTransform refTrans= child.GetComponent<ReferenceTransform>();
			if(refTrans == null) continue;
			else if (refTrans.referenceObject == target && child.GetComponent<SubstanceResourceBar>() == null)
			{
				refTrans.referenceObject = null;
				PoolManager.Pools["Selection"].Despawn (child);
			}
		}

		foreach (Transform child in trnsPanelUnitStats)
		{
			if (child.GetComponent<HealthBar>())
			{
				if (child.GetComponent<HealthBar>().Target == (target.GetComponent<IStats> () as IHealthObservable))
				{					
					DespawnBtn(child);					
				}
			}			
		}
	}

	public void DestroyHealthBar (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			ReferenceTransform refTrans= child.GetComponent<ReferenceTransform>();
			if(refTrans == null) continue;
			if (refTrans.referenceObject == target && child.GetComponent<SubstanceHealthBar>() != null)
			{
				refTrans.referenceObject = null;
				PoolManager.Pools["Selection"].Despawn (child);
			}
		}
	}
	
	public void DestroyResourceBar (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			ReferenceTransform refTrans= child.GetComponent<ReferenceTransform>();
			if(refTrans == null) continue;
			if (refTrans.referenceObject == target && child.GetComponent<SubstanceResourceBar>() != null)
			{
				refTrans.referenceObject = null;
				PoolManager.Pools["Selection"].Despawn (child);
			}
		}
	}
	#endregion

	#region Button Creation Methods

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

		Transform button = GetButtonInHUD (buttonName, buttonParent);

		button.name = buttonName;
		button.localPosition = bs.position;
		button.localScale = Vector3.one;

		PersonalizedCallbackButton pcb = button.GetComponent<PersonalizedCallbackButton>();

		if ( pcb == null )
		{
			pcb = button.gameObject.AddComponent<PersonalizedCallbackButton>();
			pcb.Init(bs.ht, bs.onClick, bs.onPress, bs.onSliderChange, bs.onActivate, bs.onRepeatClick, bs.onDrag, bs.onDrop);
		}
		else
			pcb.Init(bs.ht, bs.onClick, bs.onPress, bs.onSliderChange, bs.onActivate, bs.onRepeatClick, bs.onDrag, bs.onDrop);
	}

	Transform GetButtonInHUD (string buttonName, Transform parentButton)
	{		
		Transform button = parentButton.Find(buttonName);
		if (button == null)	button = PoolManager.Pools["Buttons"].Spawn(pref_button, parentButton); 
		return button;
	}
	#endregion

	#region Destroy, Remove e Dequeue Buttons

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

	public void RemoveButtonInInspector(string buttonName, Transform buttonParent = null)
	{
		Transform trns = (buttonParent != null) ? buttonParent : trnsOptionsMenu;
	
		foreach (Transform child in trns)
		{
			if (child.gameObject.name.Equals(buttonName) ||
				child.gameObject.name.Equals(PERSIST_STRING + buttonName))
			{
				DespawnBtn(child);		
				break;
			}
		}
	}

	public void DestroyOptionsBtns ()
	{
		IsDestroying = true;

		foreach (Transform child in trnsOptionsMenu)
		{			
			DespawnBtn(child);
		}
	}

	public bool isDestroyingQueue()
	{
		MessageQueue mq = messageInfoManager.GetQueue("Factory");
		if(mq.uiGrid.transform.childCount != 0 || isClearing)
		{
			foreach(Transform t in mq.uiGrid.transform)
			{
				DespawnBtn(t);
			}
			
			isClearing = false;
			return true;
		}
		else return false;

	}

	public void DestroyInspector (string type)
	{
		isClearing= true;
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
	#endregion

	#region CreatefeedBack

	public void CreateFeedback (Feedbacks feedback, Transform transform, float size, Color color)
	{
		CreateFeedback (feedback, transform.position, size, color);
		oldFeedback.transform.parent = transform;
	}

	public void CreateFeedback (Feedbacks feedback, Vector3 position, float size, Color color)
	{
		Transform newFeedback   = null;
		GameObject pref_feedback = null;
		float circlePs	=1f;
		float linePs	=1f;

		switch (feedback)
		{
			case Feedbacks.Move:
				pref_feedback = pref_moveFeedback;
				circlePs = 8f;
				linePs 	 = 2f;
			break;
			case Feedbacks.Self:
				pref_feedback = pref_selfFeedback;
				circlePs = 2f;
				linePs 	 = 2f;
			break;
			case Feedbacks.Attack:
				pref_feedback = pref_attackFeedback;
				circlePs = 5f;
			break;
			case Feedbacks.Invalid:
				pref_feedback = pref_invalidFeedback;
				circlePs = 3f;
			break;
		}

		newFeedback = PoolManager.Pools["Selection"].Spawn (pref_feedback, position + offesetFeedback, Quaternion.identity);
		newFeedback.transform.eulerAngles = new Vector3 (90,0,0);
		newFeedback.renderer.material.SetColor ("_TintColor", color);
		float duration = 2;

		foreach (ParticleSystem ps in newFeedback.GetComponentsInChildren<ParticleSystem>())
		{
			if(ps.gameObject.name == "circle")
			{
				ps.startSize = Mathf.Clamp (size * circlePs, 2, 10);
			}
			if(ps.gameObject.name == "line")
			{
				ps.startSize = Mathf.Clamp (size * linePs, 2, 10);
			}

			ps.renderer.material.SetColor ("_TintColor", color);
			ps.startColor = color;
			duration = ps.duration;
		}

		oldFeedback = newFeedback;
		PoolManager.Pools["Selection"].Despawn(newFeedback, duration);
	}
	#endregion

	#region InfoBox
	
	public void OpenInfoBoxUnit (Unit unit, bool techAvailable)
	{
		if (!techAvailable)
		{
			infoReq.gameObject.SetActive(true);
			reqLabel = infoReq.FindChild("req-label").GetComponent<UILabel>();
			reqLabel.text =  unit.requisites;
		}
		else

			infoReq.gameObject.SetActive(false);

		attackLabel = infoUnit.FindChild ("attack-label").GetComponent<UILabel> ();
		hpLabel	    = infoUnit.FindChild ("hp-label").GetComponent<UILabel> ();
		speedLabel  = infoUnit.FindChild ("speed-label").GetComponent<UILabel> ();
		unitsLabel  = infoUnit.FindChild ("house-label").GetComponent<UILabel> ();
		timeLabel   = infoUnit.FindChild ("time-label").GetComponent<UILabel> ();
		defLabel    = infoUnit.FindChild ("def-label").GetComponent<UILabel> ();
		descriptLabel = infoUnit.FindChild ("descript-label").GetComponent<UILabel> ();		
		bonusAtkLabel = infoUnit.FindChild ("attack-bonus").GetComponent<UILabel> ();
		bonusDefLabel = infoUnit.FindChild ("def-bonus").GetComponent<UILabel> ();
		bonusSpdLabel = infoUnit.FindChild ("speed-bonus").GetComponent<UILabel> ();
		skillLabel =  infoUnit.FindChild ("skill-label").GetComponent<UILabel> ();
		bonusAtkLabel.text = "+" + unit.bonusForce.ToString();
		bonusDefLabel.text = "+" + unit.bonusDefense.ToString();
		bonusSpdLabel.text = "+" + unit.bonusSpeed.ToString();
		skillLabel.text = unit.unitSkill.ToString();
		nameLabel.text = unit.category;				
		attackLabel.text = unit.force.ToString ();
		defLabel.text = unit.defense.ToString ();				
		hpLabel.text = unit.maxHealth.ToString ();
		speedLabel.text = unit.normalSpeed.ToString();
		unitsLabel.text = unit.numberOfUnits.ToString ();
		timeLabel.text = unit.timeToSpawn.ToString ()+"s";
		descriptLabel.text = unit.description;			
//		unit.RegisterDeathObserver (this);		
		trnsPanelInfoBox.gameObject.SetActive (true);
		infoUnit.gameObject.SetActive (true);
		infoUpgrade.gameObject.SetActive (false);
		infoFactory.gameObject.SetActive (false);
	}

	public void OpenInfoBoxFactory (FactoryBase factory, bool techAvailable) // inserida boleana para ativar requires;
	{
		if (!techAvailable)
		{
			infoReq.gameObject.SetActive(true);
			reqLabel = infoReq.FindChild("req-label").GetComponent<UILabel>();
			reqLabel.text = factory.requisites;
		}
		else
			infoReq.gameObject.SetActive(false);

		nameLabel.text = factory.category;
		hpLabel = infoFactory.FindChild ("hp-label").GetComponent<UILabel> ();
		hpLabel.text = factory.MaxHealth.ToString();
		defLabel = infoFactory.FindChild ("def-label").GetComponent<UILabel> ();
		defLabel.text = factory.defense.ToString();
		bonusDefLabel = infoFactory.FindChild ("def-bonus").GetComponent<UILabel> ();
		bonusDefLabel.text = "+" + factory.bonusDefense.ToString();
		descriptLabel = infoFactory.FindChild ("descript-label").GetComponent<UILabel> ();
		descriptLabel.text = factory.description;
		infoUnit.gameObject.SetActive (false);
		infoUpgrade.gameObject.SetActive (false);
		infoFactory.gameObject.SetActive (true);
		trnsPanelInfoBox.gameObject.SetActive (true);
	}	

	public void OpenInfoBoxUpgrade (Upgrade upgrade, bool techAvailable)
	{
		if (!techAvailable)
		{
			infoReq.gameObject.SetActive(true);
			reqLabel = infoReq.FindChild("req-label").GetComponent<UILabel>();
			reqLabel.text = upgrade.requisites;
		}
		else
			infoReq.gameObject.SetActive(false);

		infoUnit.gameObject.SetActive (false);
		infoFactory.gameObject.SetActive (false);		
		nameLabel.text = upgrade.upgradeName;
		stats1 = infoUpgrade.FindChild ("stats1-label").GetComponent<UILabel> ();
		stats1.text = upgrade.stats1Value;
		stats1Text = infoUpgrade.FindChild ("stats1-text").GetComponent<UILabel> ();
		stats1Text.text = upgrade.stats1Text;				
		stats2 = infoUpgrade.FindChild ("stats2-label").GetComponent<UILabel> ();
		stats2.text = upgrade.stats2Value;
		stats2Text = infoUpgrade.FindChild ("stats2-text").GetComponent<UILabel> ();
		stats2Text.text = upgrade.stats2Text;		
		timeLabel = infoUpgrade.FindChild ("time-label").GetComponent<UILabel> ();
		timeLabel.text = upgrade.timeToSpawn.ToString()+"s";		
		descriptLabel = infoUpgrade.FindChild ("descript-label").GetComponent<UILabel> ();
		descriptLabel.text = upgrade.description;		
		infoUnit.gameObject.SetActive (false);
		infoUpgrade.gameObject.SetActive (true);
		infoFactory.gameObject.SetActive (false);
		trnsPanelInfoBox.gameObject.SetActive (true);
	}	

	public void CloseInfoBox ()
	{
		infoUnit.gameObject.SetActive (false);
		infoUpgrade.gameObject.SetActive (false);
		infoFactory.gameObject.SetActive (false);
		trnsPanelInfoBox.gameObject.SetActive (false);
	}

	public void DespawnBtn(Transform btnTrns)
	{
		btnTrns.parent = PoolManager.Pools["Buttons"].group;
		PoolManager.Pools["Buttons"].Despawn (btnTrns);		
	}
	#endregion

	#region HealthBar
	
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
	#endregion

	#region IDeathObserver implementation

	public void OnObservableDie (GameObject dyingGO)
	{
		if (dyingGO.name.Equals (cGODisplayedOnInfoBox))
		{
			CloseInfoBox ();
		}
	}

	#endregion
}