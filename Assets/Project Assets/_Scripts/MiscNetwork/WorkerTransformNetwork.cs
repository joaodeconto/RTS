using UnityEngine;
using System.Collections;

public class WorkerTransformNetwork : Photon.MonoBehaviour
{
    Worker workerScript;
	
    void Awake ()
	{
		Init ();
	}
	
    public virtual void Init ()
    {
		if (PhotonNetwork.offlineMode)
		{
			enabled = false;
		}
		else
		{
			workerScript = GetComponent <Worker> ();
			
	        gameObject.name = gameObject.name + photonView.viewID;
			
			if (workerScript.IsNetworkInstantiate) enabled = !photonView.isMine;
			else enabled = !Visiorama.ComponentGetter.Get<GameplayManager>().IsSameTeam(workerScript);
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
