using UnityEngine;
using System.Collections;

public class DefaultCallbackButton : MonoBehaviour
{
	public delegate void OnClickDelegate(Hashtable ht);
	public delegate void OnPressDelegate(Hashtable ht, bool isDown);
	public delegate void OnDragDelegate (Hashtable ht, Vector2 delta);
	public delegate void OnDropDelegate (Hashtable ht, GameObject drag);

	public Hashtable hashtable;

	OnClickDelegate onClickDelegate;
	OnPressDelegate onPressDelegate;
	OnDragDelegate onDragDelegate;
	OnDropDelegate onDropDelegate;

	public void Init(Hashtable ht,
					 OnClickDelegate onClick = null,
					 OnPressDelegate onPress = null,
					 OnDragDelegate onDrag = null,
					 OnDropDelegate onDrop = null)
	{
		hashtable = ht;

		onClickDelegate = onClick;
		onPressDelegate = onPress;
		onDragDelegate  = onDrag;
		onDropDelegate  = onDrop;
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
}
