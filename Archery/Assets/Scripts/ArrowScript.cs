using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour {
	Rigidbody rb;
	bool didCollide = false;


	void Awake () {
		rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Destroy (this.gameObject, 4f);
		if (!didCollide) {
			transform.rotation = Quaternion.LookRotation (rb.velocity);
		}
	}
    
	void OnCollisionEnter(Collision col) {
		didCollide = true;
	}
}
