using UnityEngine;
using System.Collections;

public class WorkerTransformNetwork : Photon.MonoBehaviour
{
    Worker workerScript;
	private Vector3 correctPlayerPos; 
	private Quaternion correctPlayerRot;
	private Vector3 lastPlayerPos; 
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
			workerScript = GetComponent <Worker> ();			
	        gameObject.name = gameObject.name + photonView.viewID;
			
			if (workerScript.IsNetworkInstantiate)	enabled = !photonView.isMine;

			else 	enabled = !photonView.isMine;
		}
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
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * interpolationPos);
			transform.rotation = correctPlayerRot;
			workerScript.SyncAnimation ();
		}

    }
}
