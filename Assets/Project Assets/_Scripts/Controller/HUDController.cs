using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;
using Visiorama.Utils;

public class HUDController : MonoBehaviour
{
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

	public GameObject healthBar;
	public GameObject selectedObject;
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

	private MessageInfoManager messageInfoManager;

	public void Init()
	{
		messageInfoManager = ComponentGetter.Get<MessageInfoManager>();
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

		child.AddComponent<UIFollowTarget>().target = target.FindChild (referenceChild).transform;

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
                                        DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		GameObject button = NGUITools.AddChild ( trnsOptionsMenu.gameObject,
													pref_button);

		button.name = buttonName;
		button.transform.localPosition = position;

		if(!string.IsNullOrEmpty(textureName))
		{
			Transform trnsForeground = button.transform.FindChild("Foreground");
			ChangeButtonForegroundTexture(trnsForeground, textureName);
		}

		DefaultCallbackButton dcb = button.AddComponent<DefaultCallbackButton>();
		dcb.Init(ht, onClick, onPress, onDrag, onDrop);
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

	public void DestroyInspector ()
	{
		foreach (Transform child in trnsOptionsMenu)
		{
			Destroy (child.gameObject);
		}

		messageInfoManager.ClearQueue(FactoryBase.FactoryQueueName);
		messageInfoManager.ClearQueue(Unit.UnitGroupQueueName);
	}
}
