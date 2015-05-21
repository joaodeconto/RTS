using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class UnitTransformNetwork : Photon.MonoBehaviour
{
    Unit unitScript;
	private Vector3 correctPlayerPos; 
	private Quaternion correctPlayerRot;
	private bool wasInitialized = false;	
	public float interpolationPos = 3f;

	void Awake ()
	{
		if(!wasInitialized)	Init ();
	}
	
	public void Init ()
	{
		if(wasInitialized) return;
		wasInitialized = true;
		correctPlayerPos = transform.position; 
		correctPlayerRot = transform.rotation;
		
		if (PhotonNetwork.offlineMode)	enabled = false;

		else
		{
			unitScript = GetComponent <Unit> ();			
			gameObject.name = gameObject.name + photonView.viewID;

			if (unitScript.IsNetworkInstantiate)	enabled = !photonView.isMine;			
			else 	enabled = !photonView.isMine;
		}
	}

    void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
		if (stream.isWriting)
        {
            //We own this player: send the others our data
			stream.SendNext (unitScript.Health);
            stream.SendNext ((int)unitScript.unitState);
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
        }
        else
        {
            //Network player, receive data
			unitScript.SetHealth ((int)stream.ReceiveNext ());
            unitScript.unitState = (Unit.UnitState)(int)stream.ReceiveNext ();
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
			unitScript.SyncAnimation ();
		}
    }
}
