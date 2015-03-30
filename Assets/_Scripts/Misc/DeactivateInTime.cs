using UnityEngine;
using System.Collections;

public class DeactivateInTime : MonoBehaviour {

	public float timeToDisable;
	private float timer = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(this.gameObject.activeSelf)
		{
			timer += Time.deltaTime;
			if (timer > timeToDisable) 
			{
				this.gameObject.SetActive(false);
				timer = 0f;
			}
		}
	}
}
