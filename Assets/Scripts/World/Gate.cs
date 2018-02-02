using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour {

    [SerializeField]
    private float energyCap = 30f;
    private float energy;
    [SerializeField]
    private float drainRate = 5f;

    // Use this for initialization
    void Start() {
        energy = energyCap;
    }

    void FixedUpdate() {
        if (!DrainEnergy()) {
            Destroy(gameObject);
        }
    }

    private bool DrainEnergy() {

        return SetEnergy(energy - drainRate * Time.deltaTime);
    }

    public float GetEnergy() { return energy; }
    public bool SetEnergy(float e) {
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
