using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;
using Visiorama.Utils;

public class HUDController : MonoBehaviour
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
	public GameObject pref_selectedObject;

	public UIRoot uiRoot;
	public Transform mainTranformSelectedObjects;
	public Transform trnsOptionsMenu;
	public Transform trnsPanelInfoBox;

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
	private bool IsDestroying
	{
		get
		{
			if(_isDestroying == true)
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
				return _isDestroying = hasChild;
			}
			else return _isDestroying;
		}
		set
		{
			_isDestroying = value;
		}
	}

	public void Init()
	{
		messageInfoManager = ComponentGetter.Get<MessageInfoManager>();
		touchController    = ComponentGetter.Get<TouchController>();
		//prefabCache        = ComponentGetter.Get<PrefabCache>();

		stackButtonToCreate = new Stack<ButtonStatus>();

		IsDestroying = false;
	}

	public HealthBar CreateHealthBar (Transform target, int maxHealth, string referenceChild)
	{
		if (HUDRoot.go == null || pref_healthBar == null)
		{
			return null;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, pref_healthBar);

		if (child.GetComponent<HealthBar> () == null) child.AddComponent <HealthBar> ();
		if (child.GetComponent<UISlider> () == null) child.AddComponent <UISlider> ();

		NGUIUtils.AdjustSlider (child.GetComponent<UISlider> (), new Vector2(Mathf.CeilToInt (maxHealth * 0.6f), 10f), "Background");				

		child.AddComponent<UIFollowTarget>().target      = target.FindChild (referenceChild).transform;
		child.GetComponent<UIFollowTarget>().mGameCamera = touchController.mainCamera;
		child.GetComponent<UIFollowTarget>().mUICamera   = uiRoot.transform.FindChild ("CameraHUD").camera;

		return child.GetComponent<HealthBar> ();
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

	public void DestroySelected (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			if (child.GetComponent<ReferenceTransform>().referenceObject == target)
			{
				DestroyObject (child.gameObject);
			}
		}

		foreach (Transform child in HUDRoot.go.transform)
		{
			if (child.GetComponent<HealthBar>().target == target.GetComponent<IStats> ())
			{
				Destroy (child.gameObject);
			}
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

		Transform trns = trnsOptionsMenu.Find(buttonName);
		GameObject button = null;

		if (trns != null)
			button = trns.gameObject;
		else
			//button = prefabCache.Get(trnsOptionsMenu, "Button");
			button = NGUITools.AddChild(trnsOptionsMenu.gameObject,
										pref_button);

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

	public void RemoveButtonInInspector(string buttonName)
	{
		foreach (Transform child in trnsOptionsMenu)
		{
			if (child.gameObject.name.Equals(buttonName) ||
				child.gameObject.name.Equals(PERSIST_STRING + buttonName))
			{
				Destroy (child.gameObject);
				break;
			}
		}
	}

	public void DestroyInspector (string type)
	{
		IsDestroying = true;
		foreach (Transform child in trnsOptionsMenu)
		{
			if (!child.gameObject.name.Contains(PERSIST_STRING))
				Destroy (child.gameObject);
		}
		
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

		GameObject newFeedback;
		if (feedback == Feedbacks.Move)
		{
			newFeedback = Instantiate (pref_moveFeedback, position + offesetFeedback, Quaternion.identity) as GameObject;


		}
		else if (feedback == Feedbacks.Self)
		{
			newFeedback = Instantiate (pref_selfFeedback, position + offesetFeedback, Quaternion.identity) as GameObject;

		}
		else
		{
			newFeedback = Instantiate (pref_attackFeedback, position + offesetFeedback, Quaternion.identity) as GameObject;

		}

		newFeedback.name = "Feedback";
		newFeedback.transform.localScale = new Vector3(size * newFeedback.transform.localScale.x, 
													   size * newFeedback.transform.localScale.y,
													   size * newFeedback.transform.localScale.x);
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
	
	public void OpenInfoBox (IStats stat)
	{
		RemoveEnqueuedButtonInInspector (stat.name, Unit.UnitGroupQueueName);
		
		trnsPanelInfoBox.gameObject.SetActive (true);
		
		Unit unit = stat as Unit;
		
		Transform nameLabel = trnsPanelInfoBox.FindChild ("name-label");
		nameLabel.GetComponent<UILabel> ().text = stat.category;
		
		Transform spriteUnit = trnsPanelInfoBox.FindChild ("sprite-unit");
		Debug.Log (unit.guiTextureName);
		spriteUnit.GetComponent<UISprite> ().spriteName = unit.guiTextureName;
		
		// Info
		Transform info = trnsPanelInfoBox.FindChild ("info");
		
		Transform attackLabel = info.FindChild ("attack-label");
		attackLabel.GetComponent<UILabel> ().text = unit.AdditionalForce != 0 ?
			unit.force + "(+" + unit.AdditionalForce + ")" :
			unit.force.ToString ();
		
		Transform hpLabel = info.FindChild ("hp-label");
		hpLabel.GetComponent<UILabel> ().text = stat.Health.ToString ();
		
		Transform speedLabel = info.FindChild ("speed-label");
		speedLabel.GetComponent<UILabel> ().text = ((int)unit.pathfind.speed).ToString ();
		
		Transform unitsLabel = info.FindChild ("units-label");
		unitsLabel.GetComponent<UILabel> ().text = unit.numberOfUnits.ToString ();
		
		Transform timeLabel = info.FindChild ("time-label");
		timeLabel.GetComponent<UILabel> ().text = unit.timeToSpawn.ToString ();
		
		Transform goldLabel = info.FindChild ("gold-label");
		goldLabel.GetComponent<UILabel> ().text = unit.costOfResources.ToString ();
	}
	
	public void CloseInfoBox ()
	{
		trnsPanelInfoBox.gameObject.SetActive (false);
	}
}
