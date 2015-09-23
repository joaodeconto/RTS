using UnityEngine;
using Visiorama;
using System.Collections;

public class BtnPurchases : MonoBehaviour {

	public StoreManager sm;
	// Use this for initialization
	void Start () {
	
		 sm = ComponentGetter.Get<StoreManager>();
	}
}
