using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class WorkerTransformNetwork : Photon.MonoBehaviour
{
    Worker workerScript;

    void Awake()
    {
		workerScript = GetComponent <Worker> ();
		
        gameObject.name = gameObject.name + photonView.viewID;
		
		enabled = !photonView.isMine;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext((int)workerScript.unitState);
            stream.SendNext((int)workerScript.workerState);
            stream.SendNext(workerScript.resourceId);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation); 
        }
        else
        {
            //Network player, receive data
            workerScript.unitState = (Unit.UnitState)(int)stream.ReceiveNext();
            workerScript.workerState = (Worker.WorkerState)(int)stream.ReceiveNext();
            workerScript.resourceId = (int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        //Update remote player (smooth this, this looks good, at the cost of some accuracy)
        transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
        transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
		
		workerScript.SyncAnimation ();
    }
}
