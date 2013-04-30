using UnityEngine;
using System.Collections;

public class DefaultCallbackButton : MonoBehaviour
{
	public delegate void OnClickDelegate(Hashtable ht_dcb);
	public delegate void OnPressDelegate(Hashtable ht_dcb, bool isDown);
	public delegate void OnDragDelegate (Hashtable ht_dcb, Vector2 delta);
	public delegate void OnDropDelegate (Hashtable ht_dcb, GameObject drag);
	public delegate void OnRepeatClickDelegate (Hashtable ht_dcb);

	public Hashtable hashtable;

	OnClickDelegate onClickDelegate;
	OnPressDelegate onPressDelegate;
	OnDragDelegate onDragDelegate;
	OnDropDelegate onDropDelegate;
	OnRepeatClickDelegate onRepeatClickDelegate;

	public void Init(Hashtable ht,
					 OnClickDelegate onClick = null,
					 OnPressDelegate onPress = null,
					 OnDragDelegate onDrag = null,
					 OnDropDelegate onDrop = null,
					 OnRepeatClickDelegate onRepeatClick = null)
	{
		ChangeParams(ht, onClick, onPress, onDrag, onDrop, onRepeatClick);
	}

	public void ChangeParams(Hashtable ht,
							 OnClickDelegate onClick = null,
							 OnPressDelegate onPress = null,
							 OnDragDelegate onDrag = null,
							 OnDropDelegate onDrop = null,
							 OnRepeatClickDelegate onRepeatClick = null)
	{
		hashtable 			  = ht;
		onClickDelegate 	  = onClick;
		onPressDelegate 	  = onPress;
		onDragDelegate  	  = onDrag;
		onDropDelegate  	  = onDrop;
		onRepeatClickDelegate = onRepeatClick;
	}

	void OnClick ()
	{
		if(onClickDelegate != null)
			onClickDelegate(hashtable);
	}

	void OnPress (bool isDown)
	{
		if(onPressDelegate != null)
			onPressDelegate(hashtable, isDown);
		
		if (onRepeatClickDelegate != null)
		{
			if (isDown)
			{
				InvokeRepeating ("OnRepeatClick", 0f, 0.01f);
			}
			else
			{
				CancelInvoke ("OnRepeatClick");
			}
		}
	}

	void OnDrag (Vector2 delta)
	{
		if(onDragDelegate != null)
			onDragDelegate(hashtable, delta);
	}

	void OnDrop (GameObject drag)
	{
		if(onDropDelegate != null)
			onDropDelegate(hashtable, drag);
	}
	
	void OnRepeatClick ()
	{
		onRepeatClickDelegate (hashtable);
	}
}
