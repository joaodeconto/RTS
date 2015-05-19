using UnityEngine;
using System.Collections;

public class WorkerTransformNetwork : Photon.MonoBehaviour
{
    Worker workerScript;
	private Vector3 correctPlayerPos; 
	private Quaternion correctPlayerRot;
	private bool wasInitialized = false; 

	
	void Awake ()
	{
		if (PhotonNetwork.offlineMode)	enabled = false;
		else	Invoke("Init",0.2f);
	}

	public void Init ()
	{
		if(wasInitialized) return;
		wasInitialized = true;
		correctPlayerPos = transform.position; 
		correctPlayerRot = transform.rotation;
		gameObject.name = gameObject.name + photonView.viewID;
    }

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
        {
            //We own this player: send the others our data
			stream.SendNext (workerScript.Health);
            stream.SendNext ((int)workerScript.unitState);
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
            stream.SendNext ((int)workerScript.workerState);
            stream.SendNext (workerScript.resourceId);
        }
        else
        {
            //Network player, receive data
            workerScript.SetHealth((int)stream.ReceiveNext ());
            workerScript.unitState = (Unit.UnitState)(int)stream.ReceiveNext ();
            correctPlayerPos = (Vector3)stream.ReceiveNext ();
            correctPlayerRot = (Quaternion)stream.ReceiveNext ();
            workerScript.workerState = (Worker.WorkerState)(int)stream.ReceiveNext();
            workerScript.resourceId = (int)stream.ReceiveNext();
        }
	}

    void Update()
    {
		if (!photonView.isMine)
		{
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 3);
			transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 3);
			workerScript.SyncAnimation ();
		}

    }
}
