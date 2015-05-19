using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class RangeUnitTransformNetwork : Photon.MonoBehaviour
{
    RangeUnit rangeUnitScript;
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
			rangeUnitScript = GetComponent<RangeUnit>();
			
			gameObject.name = gameObject.name + photonView.viewID;
			
			if (rangeUnitScript.IsNetworkInstantiate)
			{
				enabled = !photonView.isMine;
				Debug.Log("pelo photonview  " + enabled);
			}
			
			else 
			{
				enabled = !photonView.isMine;
				Debug.Log("pelo gameplay  " + enabled);
				
			}
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

//    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
//    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
       	if (!photonView.isMine)
		{
       		transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
     		transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
			rangeUnitScript.SyncAnimation ();
		}
    }
}

