using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class RangeUnitTransformNetwork : Photon.MonoBehaviour
{
    RangeUnit rangeUnitScript;
	private Vector3 correctPlayerPos; 
	private Quaternion correctPlayerRot;
	private bool wasInitialized = false;	
	public float interpolationPos = 3f;
	
	void Awake ()
	{
		if(!wasInitialized)
		{
			Init ();
		}
	}
	
	public void Init ()
	{
		if(wasInitialized) return;
		wasInitialized = true;
		correctPlayerPos = transform.position; 
		correctPlayerRot = transform.rotation;
		
		if (PhotonNetwork.offlineMode)
		{
			enabled = false;
		}
		else
		{
			rangeUnitScript = GetComponent<RangeUnit>();			
			gameObject.name = gameObject.name + photonView.viewID;			
			if (rangeUnitScript.IsNetworkInstantiate)	enabled = !photonView.isMine;
			else 	enabled = !photonView.isMine;
		}
	}
	
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
        {
            //We own this player: send the others our data
			stream.SendNext ((int)rangeUnitScript.Health);
			stream.SendNext ((int)rangeUnitScript.projectileAnimating);
			stream.SendNext ((int)rangeUnitScript.unitState);
			stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);

        }
        else
        {
            //Network player, receive data
			rangeUnitScript.SetHealth ((int)stream.ReceiveNext ());
			rangeUnitScript.projectileAnimating = (int)stream.ReceiveNext ();
			rangeUnitScript.unitState = (Unit.UnitState)(int)stream.ReceiveNext ();
            correctPlayerPos = (Vector3)stream.ReceiveNext ();
            correctPlayerRot = (Quaternion)stream.ReceiveNext ();
        }
	}

	void Update()
    {
       	if (!photonView.isMine)
		{
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * interpolationPos);
			transform.rotation = correctPlayerRot;
			rangeUnitScript.SyncAnimation ();
		}
    }
}

