using UnityEngine;
using System.Collections;


public class RangeObjectTransformNetwork : Photon.MonoBehaviour
{
    RangeObject rangeObjectScript;

	private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
	private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

	
    void Awake ()
	{
		Init ();
		correctPlayerPos = transform.position;
		correctPlayerRot = transform.rotation;
	}

    public void Init ()
    {
		if (PhotonNetwork.offlineMode)
		{
			enabled = false;
		}
		else
		{
			rangeObjectScript = GetComponent <RangeObject> ();
			
	        gameObject.name = gameObject.name + photonView.viewID;
			
//			if (rangeObjectScript.IsNetworkInstantiate) enabled = !photonView.isMine;
//
//			else
//			{
//				GameplayManager gm = Visiorama.ComponentGetter.Get<GameplayManager>();
//				
//				enabled = !gm.IsSameTeam(rangeObjectScript);
//				
//				if (gm.IsBotTeam (rangeObjectScript) && PhotonNetwork.isMasterClient)
//				{
//					enabled = false;
//				}
//			}
		}
	}


	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
        }
        else
        {
            //Network player, receive data
            correctPlayerPos = (Vector3)stream.ReceiveNext ();
            correctPlayerRot = (Quaternion)stream.ReceiveNext ();
        }
	}
	   
    void Update()
    {
		transform.position = Vector3.Lerp(transform.position, correctPlayerPos, 5f);
		transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, 5f);
			
    }
}

