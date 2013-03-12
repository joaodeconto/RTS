using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class HUDController : MonoBehaviour
{
	[System.Serializable]
	public class NamePosition2D
	{
		public string Name;
		public Vector2 Position;
	}

	public GameObject healthBar;
	public GameObject selectedObject;
	public Transform mainTranformSelectedObjects;
	public Transform transformMenu;
	public GameObject pref_button;

	public NamePosition2D MainQueue;

	private List<UIGrid> gridsToReposition = new List<UIGrid>();

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
		refTransform.offsetPosition += Vector3.up * 0.4f;

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
												Vector2 rootPosition,
												bool verticalEnqueue,
												int maxPerLine,
												int maxItems,
												Hashtable ht,
												string textureName = "",
												DefaultCallbackButton.OnClickDelegate onClick = null,
												DefaultCallbackButton.OnPressDelegate onPress = null,
												DefaultCallbackButton.OnDragDelegate onDrag = null,
												DefaultCallbackButton.OnDropDelegate onDrop = null)
	{
		GameObject queue = GetQueueGameObject(queueName, rootPosition);
		UIGrid uiGrid = queue.GetComponent<UIGrid>();

		GameObject button = NGUITools.AddChild (queue,
												pref_button);

		button.name = buttonName;

		Vector3 position = Vector3.zero;

		//TODO refazer essa parte
		uiGrid.cellWidth  = uiGrid.cellHeight = 50;
		uiGrid.maxPerLine = maxPerLine;
		uiGrid.sorted = true;

		if(verticalEnqueue)
		{
			//TODO
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
		}
		else
		{
			uiGrid.arrangement = UIGrid.Arrangement.Horizontal;
			//FIXME
			//position = new Vector3 (rootPosition.x + ((index - 1) * texture.width), rootPosition.y, -5);
		}

		button.transform.localPosition = position;

		if(!string.IsNullOrEmpty(textureName))
		{
			Transform trnsForeground = button.transform.FindChild("Foreground");
			ChangeButtonForegroundTexture(trnsForeground, textureName);
		}
		//UnitCallbackButton ucb = newButton.AddComponent<UnitCallbackButton> ();
		//ucb.Init (unit, factory);

		DefaultCallbackButton dcb = button.AddComponent<DefaultCallbackButton>();

		dcb.Init(ht, onClick, onPress, onDrag, onDrop);

		AddGridToReposition(uiGrid);
	}

	public void RemoveEnqueuedButtonInInspector(string queueName,
												string buttonName)
	{
		GameObject queue     = GetQueueGameObject(queueName);
		Transform trnsButton = queue.transform.FindChild(buttonName);

		if(trnsButton != null)
		{
			Destroy (trnsButton.gameObject);
			AddGridToReposition(queue.GetComponent<UIGrid>());
		}
	}

	public void DequeueButtonInInspector(string queueName)
	{
		GameObject queue = GetQueueGameObject(queueName);

		if(queue.transform.childCount == 0)
			return;

		int childIndex = queue.transform.childCount - 1;

		Destroy (queue.transform.GetChild(childIndex).gameObject);

		AddGridToReposition(queue.GetComponent<UIGrid>());
	}

	public bool CheckQueuedButtonIsFirst(string queueName, string buttonName)
	{
		GameObject queue     = GetQueueGameObject(queueName);
		Transform trnsButton = queue.transform.FindChild(buttonName);

		if(trnsButton == null)
		{
			return false;
		}

		Debug.Log("trnsButton.localPosition: " + trnsButton.localPosition);
		return trnsButton.localPosition == Vector3.zero;
	}

	private GameObject GetQueueGameObject(string queueName)
	{
		return GetQueueGameObject(queueName, Vector3.zero);
	}

	private GameObject GetQueueGameObject(string queueName, Vector3 rootPosition)
	{
		Transform trnsQueue = transformMenu.FindChild(queueName);
		GameObject queue = null;

		if(trnsQueue == null)
		{
			queue = new GameObject();
			queue.transform.parent = transformMenu;

			queue.name = queueName;
			queue.AddComponent<UIGrid>();

			queue.transform.localScale    = Vector3.one;
			queue.transform.localPosition = Vector3.zero;
		}
		else
		{
			queue = trnsQueue.gameObject;
		}

		if(rootPosition != Vector3.zero)
			queue.transform.localPosition = rootPosition;

		return queue;
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
		GameObject newButton = NGUITools.AddChild ( transformMenu.gameObject,
													pref_button);

		newButton.transform.localPosition = position;

		if(!string.IsNullOrEmpty(textureName))
		{
			Transform trnsForeground = newButton.transform.FindChild("Foreground");
			ChangeButtonForegroundTexture(trnsForeground, textureName);
		}

		//UnitCallbackButton ucb = newButton.AddComponent<UnitCallbackButton> ();
		//ucb.Init (unit, factory);

		DefaultCallbackButton dcb = newButton.AddComponent<DefaultCallbackButton>();
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
		foreach (Transform child in transformMenu)
		{
			Destroy (child.gameObject);
		}
	}

	private void AddGridToReposition(UIGrid uiGrid)
	{
		if(!gridsToReposition.Contains(uiGrid))
			gridsToReposition.Add(uiGrid);

		Invoke("RepositionGrid", 0.1f);
	}

	private void RepositionGrid()
	{
		while(gridsToReposition.Count != 0)
		{
			gridsToReposition[0].repositionNow = true;
			gridsToReposition.RemoveAt(0);
		}
	}
}
