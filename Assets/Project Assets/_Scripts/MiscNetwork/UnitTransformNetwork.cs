using UnityEngine;
using System.Collections;

public class UnitTransformNetwork : Photon.MonoBehaviour
{
    Unit unitScript;

    void Awake()
    {
		unitScript = GetComponent <Unit> ();
		
        gameObject.name = gameObject.name + photonView.viewID;
		
        enabled = !photonView.isMine;
		unitScript.pathfind.enabled = photonView.isMine;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext((int)unitScript.unitState);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation); 
        }
        else
        {
            //Network player, receive data
            unitScript.unitState = (Unit.UnitState)(int)stream.ReceiveNext();
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
		
		unitScript.SyncAnimation ();
    }
	
	/*
	public AnimationClip this [int index]
    {
        get
        {
            int i = 0;
            foreach(AnimationState clip in unitScript.ControllerAnimation)
            {
                if (i == index)
                {
                    return clip.clip;    
                }
                i++;    
            }
			return null;
        }
    }
    */
}
