using UnityEngine;
using System.Collections;
using Visiorama.Utils;

public class RangeObject : Photon.MonoBehaviour
{
	public delegate void RangeHitDelegate (Hashtable ht);
	protected RangeHitDelegate rangeHitDelegate;
	protected Hashtable hashtable;
	public float speed = 20f;
	public float smoothFactor = 3f;
	protected GameObject target;
	protected Vector3 targetPosition;
	protected float time;
	protected bool reachedTarget = false;
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	
	public void Init (GameObject target, float timeToDestroyWhenCollide, RangeHitDelegate rhd, Hashtable ht = null)
	{
		if (target == null)
		{
			DestroyObjectInNetwork();
			return;
		}

		enabled = true;
		rangeHitDelegate = rhd;
		hashtable = ht;
		targetPosition = target.transform.position;
		this.target = target;
		time = timeToDestroyWhenCollide;
		correctPlayerPos = transform.position;
		correctPlayerRot = transform.rotation;
		Move ();
	}
	
	void Move ()
	{
		InvokeRepeating("CheckProjectileHit",0.2f,0.1f);
		Vector3[] positionArrays = new Vector3[2]
		{
			transform.position, targetPosition
		};
		
		Vector3 center = Math.CenterOfObjects (positionArrays);
		center.y += Mathf.Sqrt(Vector3.Distance (transform.position, targetPosition));
		
		iTween.MoveTo (gameObject, 
			iTween.Hash (iT.MoveTo.position, center, 
						iT.MoveTo.speed, speed, 
						iT.MoveTo.easetype, iTween.EaseType.linear,
						iT.MoveTo.oncomplete, "MaximumHeight"
			)
		);
		
		transform.LookAt (center);
		
		iTween.RotateTo (gameObject,
			iTween.Hash (iT.RotateTo.rotation, new Vector3 (0, transform.eulerAngles.y, 
															transform.eulerAngles.z),
						iT.RotateTo.time, 0.5f,
						iT.RotateTo.easetype, iTween.EaseType.linear
			)
		);
	}
	
	void MaximumHeight ()
	{
		iTween.MoveTo (gameObject, 
			iTween.Hash (iT.MoveTo.position, targetPosition, 
						iT.MoveTo.speed, speed, 
						iT.MoveBy.easetype, iTween.EaseType.linear,
						iT.MoveTo.oncomplete, "FinalPoint"
			)
		);
		
		iTween.LookTo (gameObject,
			iTween.Hash (iT.LookTo.looktarget, targetPosition,
						iT.LookTo.time, 0.5f,
						iT.LookTo.easetype, iTween.EaseType.linear
			)
		);
	}
	
	void FinalPoint ()
	{
		reachedTarget = true;

		if (PhotonNetwork.offlineMode)
		{
			Destroy (gameObject, time);
		}
		else
		{
			Invoke ("DestroyObjectInNetwork", time);
		}
	}
	
	void DestroyObjectInNetwork ()
	{
		PhotonNetwork.Destroy (gameObject);
	}

	void CheckProjectileHit()
	{
		bool isIntersects = false;		
		
		if (target.collider != null) isIntersects = target.collider.bounds.Intersects (collider.bounds);

		else
		{
			isIntersects = Math.AABBContains (target.transform.position,collider.bounds, Math.IgnoreVector.Y);
		}
		
		if (isIntersects)
		{
			rangeHitDelegate (hashtable);
			CancelInvoke("CheckProjectileHit");
		}		

	}
	
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
		if (stream.isWriting)
        {
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
        }
        else
        {
            correctPlayerPos = (Vector3)stream.ReceiveNext ();
            correctPlayerRot = (Quaternion)stream.ReceiveNext ();
        }
    }

    void Update ()
    {
		if(!reachedTarget)
		{
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * smoothFactor);
			transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * smoothFactor);
    	}
	}
}