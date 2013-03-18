using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageInfoManager : MonoBehaviour
{
	[System.Serializable]
	public class MessageQueueAttributes
	{
		public string queueName;
		public Vector2 rootPosition;
		public bool IsVerticalQueue;
		public int maxPerLine;
		public int maxItems;
	}

	public GameObject pref_button;
	public Transform transformPanelMenu;

	public MessageQueueAttributes[] messageQueuesAttributes;

	private MessageQueue[] messageQueues;

	private bool wasInitialized;

	public MessageInfoManager Init()
	{
		if(wasInitialized)
			return this;

		wasInitialized = true;

		messageQueues = new MessageQueue [messageQueuesAttributes.Length];
		for(int i = messageQueues.Length - 1; i != -1; --i)
		{
			UIGrid uiGrid = GetQueueGrid(messageQueuesAttributes[i].queueName,
										 messageQueuesAttributes[i].rootPosition);

			messageQueues[i] = transformPanelMenu.gameObject.AddComponent<MessageQueue>();
			messageQueues[i].Init ( pref_button, uiGrid,
									messageQueuesAttributes[i].queueName,
									messageQueuesAttributes[i].rootPosition,
									messageQueuesAttributes[i].IsVerticalQueue,
									messageQueuesAttributes[i].maxPerLine,
									messageQueuesAttributes[i].maxItems);
		}

		return this;
	}

	public MessageQueue GetQueue(string queueName)
	{
		if(!wasInitialized)
			Init();

		MessageQueue messageQueue = null;

		foreach(MessageQueue mq in messageQueues)
		{
			if(mq.queueName.Equals(queueName))
			{
				messageQueue = mq;
				break;
			}
		}

		if(messageQueue == null)
		{
			Debug.Log("nao existe essa message queue: " + queueName);
			Debug.Break();
		}

		return messageQueue;
	}

	public void ClearQueue(string queueName)
	{
		MessageQueue mq = GetQueue(queueName);

		if(mq != null)
		{
			mq.Clear();
		}
	}

	private UIGrid GetQueueGrid(string queueName, Vector3 rootPosition)
	{
		Transform trnsQueue = transformPanelMenu.FindChild(queueName);
		GameObject queue = null;

		if(trnsQueue == null)
		{
			queue = new GameObject();

			queue.layer            = transformPanelMenu.gameObject.layer;
			queue.transform.parent = transformPanelMenu.transform;

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

		return queue.GetComponent<UIGrid>();
	}

	public void AdEnqueuedButtonInInspector(string buttonName,
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
   /*     GameObject queue = GetQueueGameObject(queueName, rootPosition);*/
		//UIGrid uiGrid = queue.GetComponent<UIGrid>();

		//GameObject button = NGUITools.AddChild (queue, pref_button);

		//button.name  = buttonName;
		//button.layer = queue.layer;

		//Vector3 position = Vector3.zero;

		////TODO refazer essa parte
		//uiGrid.cellWidth  = uiGrid.cellHeight = 50;
		//uiGrid.maxPerLine = maxPerLine;
		//uiGrid.sorted = true;

		//if(verticalEnqueue)
		//{
			////TODO
			//uiGrid.arrangement = UIGrid.Arrangement.Vertical;
		//}
		//else
		//{
			//uiGrid.arrangement = UIGrid.Arrangement.Horizontal;
			////FIXME
			////position = new Vector3 (rootPosition.x + ((index - 1) * texture.width), rootPosition.y, -5);
		//}

		//button.transform.localPosition = position;

		//if(!string.IsNullOrEmpty(textureName))
		//{
			//Transform trnsForeground = button.transform.FindChild("Foreground");
			//ChangeButtonForegroundTexture(trnsForeground, textureName);
		//}

		////UnitCallbackButton ucb = newButton.AddComponent<UnitCallbackButton> ();
		////ucb.Init (unit, factory);

		//DefaultCallbackButton dcb = button.AddComponent<DefaultCallbackButton>();

		//dcb.Init(ht, onClick, onPress, onDrag, onDrop);

		/*AddGridToReposition(uiGrid);*/
	}

   /* public void RemoveEnqueuedButtonInInspector(string queueName, string buttonName)*/
	//{
		//GameObject queue     = GetQueueGameObject(queueName);
		//Transform trnsButton = queue.transform.FindChild(buttonName);

		//if(trnsButton != null)
		//{
			//Destroy (trnsButton.gameObject);
			//AddGridToReposition(queue.GetComponent<UIGrid>());
		//}
	/*}*/
}
