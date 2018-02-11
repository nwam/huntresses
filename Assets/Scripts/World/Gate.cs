using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour, IPathLogic {
    
    [SerializeField]
    private float energyCap = 30f;
    private float energy;
    [SerializeField]
    private float drainRate = 5f;

    // Use this for initialization
    void Start() {
        energy = energyCap;
        OnSpawn();
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

    public float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public string MapKey() {
        return "Gate";
    }

    public void OnSpawn() {
        WorldGrid.Instance.AddToMap(this.gameObject);
    }
}
