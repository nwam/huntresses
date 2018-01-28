using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowIntoExistance : MonoBehaviour {

	[SerializeField]
	private float t = 3; 
	private float scale = 0;
	
	// Update is called once per frame
	void Update () {
		scale += Time.deltaTime / t;

		if (scale >= 1) {
			transform.localScale = new Vector3 (1, 1, 1);
			enabled = false;
		}

		transform.localScale = new Vector3 (scale, scale, 1);
	}
}
