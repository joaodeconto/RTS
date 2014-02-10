using UnityEngine;
using System.Collections;
using Visiorama.Utils;

public class RangeObject : MonoBehaviour {
	
	public delegate void RangeHitDelegate (Hashtable ht);
	
	protected RangeHitDelegate rangeHitDelegate;
	protected Hashtable hashtable;
	
	float maximumHeightDistance = 10f;
	float speed = 20f;
			
	protected GameObject target;
	protected Vector3 targetPosition;
	protected float time;
	
	public void Init (GameObject target, float timeToDestroyWhenCollide, RangeHitDelegate rhd, Hashtable ht = null)
	{
		enabled = true;
		
		rangeHitDelegate = rhd;
		hashtable = ht;
		targetPosition = target.transform.position;
		this.target = target;
		time = timeToDestroyWhenCollide;
		
		if (collider == null)
		{
			BoxCollider col = gameObject.AddComponent<BoxCollider> ();
			
			MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
			col.size = mr.bounds.size;
		}
		
		collider.isTrigger = true;
		
		Move ();
	}
	
	void Move ()
	{
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
		bool isIntersects = false;
		
		if (target.collider != null)
		{
			isIntersects = target.collider.bounds.Intersects (collider.bounds);
		}
		else
		{
			isIntersects = Math.AABBContains (target.transform.position,
											  collider.bounds,
											  Math.IgnoreVector.Y);
		}
		
		if (isIntersects)
		{
			rangeHitDelegate (hashtable);
		}
		
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

    private Vector3 correctPlayerPos = Vector3.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
	
	void Awake ()
	{
		correctPlayerPos = transform.position;
		correctPlayerRot = transform.rotation;
	}
	
    void Update ()
    {
        transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
        transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
    }
}