using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class UnitTransformNetwork : Photon.MonoBehaviour
{
    Unit unitScript;
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
		wasInitialized = true;
		correctPlayerPos = transform.position; 
		correctPlayerRot = transform.rotation;

		if (PhotonNetwork.offlineMode)
		{
			enabled = false;
		}
		else
		{
			unitScript = GetComponent <Unit> ();
			
	        gameObject.name = gameObject.name + photonView.viewID;
			
			if (unitScript.IsNetworkInstantiate) enabled = !photonView.isMine;
			else
			{
				GameplayManager gm = Visiorama.ComponentGetter.Get<GameplayManager>();
				
				enabled = !gm.IsSameTeam(unitScript);
				
				if (gm.IsBotTeam (unitScript) && PhotonNetwork.isMasterClient)
				{
					enabled = false;
				}
			}
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

//			lastNetworkDataReceivedTime = info.timestamp;
        }
    }



    void Update()
    {
		if (!photonView.isMine)
		{
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);

			unitScript.SyncAnimation ();
		}

    }

	//Tentativa de prever a posiçao da unit atraves da velocidade.
	//Nao funcionou.

//	NavMeshAgent navAgent;
//	double lastNetworkDataReceivedTime;
//	float m_Speed;
//	
//	void UpdateNetworkPosition()
//	{
//		float pingInSeconds = (float)PhotonNetwork.GetPing() * 0.001f;
//		float timesinceLastUpdate = (float)(PhotonNetwork.time - lastNetworkDataReceivedTime);
//		float totalTimePassed = pingInSeconds + timesinceLastUpdate;
//
//		Vector3 possiblePlayerPos = correctPlayerPos + transform.forward * m_Speed * totalTimePassed;
//
//		Vector3 newPosition = Vector3.MoveTowards (transform.position
//		                                           , possiblePlayerPos
//		                                           ,m_Speed * Time.deltaTime);
//
//		if(Vector3.Distance (transform.position, possiblePlayerPos) > 3f && m_Speed > 0)
//		{
//			newPosition = possiblePlayerPos;
//		}
//
//		transform.position = newPosition;
//		transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
//	}
	
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
