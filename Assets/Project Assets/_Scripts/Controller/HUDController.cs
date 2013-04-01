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

	[System.Serializable]
	public class ButtonStatus
	{
		public string buttonName;
		public Vector3 position;
		public Hashtable ht;
		public string textureName;
		public DefaultCallbackButton.OnClickDelegate onClick;
		public DefaultCallbackButton.OnPressDelegate onPress;
		public DefaultCallbackButton.OnDragDelegate onDrag;
		public DefaultCallbackButton.OnDropDelegate onDrop;
		public bool persistent;
	}

	public GameObject healthBar;
	public GameObject selectedObject;
	public UIRoot uiRoot;
	public Transform mainTranformSelectedObjects;
	public Transform trnsOptionsMenu;
	public GameObject pref_button;

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

	private TouchController touchController;
	private MessageInfoManager messageInfoManager;

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
		stackButtonToCreate = new Stack<ButtonStatus>();

		IsDestroying = false;
	}

	public HealthBar CreateHealthBar (Transform target, int maxHealth, string referenceChild)
	{
		if (HUDRoot.go == null || healthBar == null)
		{
			return null;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, healthBar);

		if (child.GetComponent<HealthBar> () == null) child.AddComponent <HealthBar> ();
		if (child.GetComponent<UISlider> () == null) child.AddComponent <UISlider> ();

		NGUIUtils.AdjustSlider (child.GetComponent<UISlider> (), new Vector2(maxHealth,
			child.GetComponent<UISlider> ().fullSize.y), "Background");

		child.AddComponent<UIFollowTarget>().target      = target.FindChild (referenceChild).transform;
		child.GetComponent<UIFollowTarget>().mGameCamera = touchController.mainCamera;
		child.GetComponent<UIFollowTarget>().mUICamera   = uiRoot.transform.FindChild ("CameraHUD").camera;

		return child.GetComponent<HealthBar> ();
	}

	public void CreateSelected (Transform target, float size, Color color)
	{
		GameObject selectObj = Instantiate (selectedObject, target.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 0.1f, 0.1f, size * 0.1f);
		selectObj.transform.GetComponent<AnimateTiledTexture>().Play ();
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.1f;

		selectObj.renderer.sharedMaterial.color = color;

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
			Destroy (child.gameObject);
		}
	}

	public void CreateEnqueuedButtonInInspector(string buttonName,
												string queueName,
												Hashtable ht,
												string textureName = "",
												DefaultCallbackButton.OnClickDelegate onClick = null,
												DefaultCallbackButton.OnPressDelegate onPress = null,
												DefaultCallbackButton.OnDragDelegate onDrag = null,
												DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		if (ht == null)
			ht = new Hashtable();

		if(!string.IsNullOrEmpty(textureName))
			ht["textureName"] = textureName;

		MessageQueue mq = messageInfoManager.GetQueue(queueName);
		mq.AddMessageInfo ( buttonName, ht,
							onClick, onPress, onDrag, onDrop);
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
			button = NGUITools.AddChild(trnsOptionsMenu.gameObject,
										pref_button);

		button.name = buttonName;
		button.transform.localPosition = bs.position;

		PersonalizedCallbackButton pcb = button.GetComponent<PersonalizedCallbackButton>();

		if ( pcb == null )
		{
			pcb = button.AddComponent<PersonalizedCallbackButton>();
			pcb.Init(bs.ht, bs.onClick, bs.onPress, bs.onDrag, bs.onDrop);
		}
		else
			pcb.ChangeParams(bs.ht, bs.onClick, bs.onPress, bs.onDrag, bs.onDrop);
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

	public void DestroyInspector ()
	{
		IsDestroying = true;
		foreach (Transform child in trnsOptionsMenu)
		{
			if (!child.gameObject.name.Contains(PERSIST_STRING))
				Destroy (child.gameObject);
		}

		messageInfoManager.ClearQueue(FactoryBase.FactoryQueueName);
		messageInfoManager.ClearQueue(Unit.UnitGroupQueueName);
	}
}
