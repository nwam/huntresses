using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour, IPathLogic {

    [SerializeField]
    private float energyCap = 30f;
    private float energy;
    private float drainRate = 1f;

	// Use this for initialization
	void Start () {
        energy = energyCap;
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

    public float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public string MapKey() {
        return "Gate";
    }
}
