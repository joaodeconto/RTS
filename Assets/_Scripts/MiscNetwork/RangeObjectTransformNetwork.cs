using UnityEngine;
using System.Collections;


public class RangeObjectTransformNetwork : Photon.MonoBehaviour
{
    RangeObject rangeObjectScript;
	private Vector3 correctPlayerPos; 
	private Quaternion correctPlayerRot;
	private bool wasInitialized = false; 
	
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
			Debug.Log("offline???  " + enabled);
		}
		else
		{
			rangeObjectScript = GetComponent <RangeObject> ();
			
			gameObject.name = gameObject.name + photonView.viewID;
			
			enabled = !photonView.isMine;

			Debug.Log("pelo gameplay  " + enabled);

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
		transform.position = Vector3.Slerp(transform.position, correctPlayerPos, 2f);
		transform.rotation = Quaternion.Slerp(transform.rotation, correctPlayerRot, 2f);
			
    }
}

