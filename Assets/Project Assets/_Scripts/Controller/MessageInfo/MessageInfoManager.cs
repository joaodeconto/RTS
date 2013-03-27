using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageInfoManager : MonoBehaviour
{
	[System.Serializable]
	public class TemporizedMessageQueueAttributes
	{
		public float timeToFadeout;
		public Color fadeColor;
	}

	[System.Serializable]
	public class MessageQueueAttributes
	{
		public string queueName;
		public Vector2 rootPosition;
		public Vector2 cellSize;
		public Vector2 padding;
		public float labelSize;
		public bool IsVerticalQueue;
		public int maxPerLine;
		public int maxItems;
		public bool IsTemporizedQueue;
		public TemporizedMessageQueueAttributes temporizedQueueAttributes;
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

			if(!messageQueuesAttributes[i].IsTemporizedQueue)
			{
				messageQueues[i] = transformPanelMenu.gameObject.AddComponent<DefaultMessageQueue>();
				DefaultMessageQueue dmq = (DefaultMessageQueue)(messageQueues[i]);

				dmq.Init(pref_button, uiGrid,
						 messageQueuesAttributes[i].queueName,
						 messageQueuesAttributes[i].rootPosition,
						 messageQueuesAttributes[i].cellSize,
						 messageQueuesAttributes[i].padding,
						 messageQueuesAttributes[i].labelSize,
						 messageQueuesAttributes[i].IsVerticalQueue,
						 messageQueuesAttributes[i].maxPerLine,
						 messageQueuesAttributes[i].maxItems);
			}
			else
			{
				messageQueues[i] = transformPanelMenu.gameObject.AddComponent<TemporizedMessageQueue>();
				TemporizedMessageQueue tmq = (TemporizedMessageQueue)(messageQueues[i]);
				tmq.Init(pref_button, uiGrid,
						 messageQueuesAttributes[i].queueName,
						 messageQueuesAttributes[i].rootPosition,
						 messageQueuesAttributes[i].cellSize,
						 messageQueuesAttributes[i].padding,
						 messageQueuesAttributes[i].labelSize,
						 messageQueuesAttributes[i].IsVerticalQueue,
						 messageQueuesAttributes[i].temporizedQueueAttributes.timeToFadeout,
						 messageQueuesAttributes[i].temporizedQueueAttributes.fadeColor,
						 messageQueuesAttributes[i].maxPerLine,
						 messageQueuesAttributes[i].maxItems);
			}
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
			if(mq.QueueName.Equals(queueName))
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
}
