using UnityEngine;
using System.Collections;

public class WorkerTransformNetwork : UnitTransformNetwork
{
    Worker workerScript;
	
    public override void Init ()
    {
		base.Init ();
		
		if (PhotonNetwork.offlineMode)
		{
			workerScript = GetComponent <Worker> ();
		}
    }

	public override void SerializeView (PhotonStream stream)
	{
		base.SerializeView (stream);
		
        if (stream.isWriting)
        {
            stream.SendNext ((int)workerScript.workerState);
            stream.SendNext (workerScript.resourceId);
        }
        else
        {
            //Network player, receive data
            workerScript.workerState = (Worker.WorkerState)(int)stream.ReceiveNext();
            workerScript.resourceId = (int)stream.ReceiveNext();
        }
	}

//    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
//    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this
//
//    void Update()
//    {
//        //Update remote player (smooth this, this looks good, at the cost of some accuracy)
//        transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
//        transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
//		
//		workerScript.SyncAnimation ();
//    }
}
