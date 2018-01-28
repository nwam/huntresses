using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour {

    private const float energyCap = 30f;
    [SerializeField]
    private float energy = energyCap;
    private float drainRate = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	void FixedUpdate () {
		if (!drainEnergy()) {
            Destroy(this.gameObject);
        }
	}

    private bool drainEnergy() {
        return this.setEnergy(energy - drainRate * Time.deltaTime);
    }

    public float getEnergy() { return energy; }
    public bool setEnergy(float e) {
        if (e < 0) {
            energy = 0;
            return false;
        }
        else {
            if (e > energyCap) {
                energy = energyCap;
            }
            else {
                energy = e;
            }
            return true;
        }
    }

}
