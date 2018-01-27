using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour {
    
    [SerializeField]
    private float bloodCapacity = 10f;
    private bool beingHarvested = false;
    [SerializeField]
    private float drainRate = 1f;

	// Use this for initialization
	void Start () {
		


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float getBloodCapacity() { return bloodCapacity; }
    public void setBloodCapacity(float bc) { bloodCapacity = bc; }

    public bool getBeingHarvested() { return beingHarvested; }
    public void setBeingHarvested(bool bh) { beingHarvested = bh; }

    public float beHarvested() {

        beingHarvested = true;
        float drained = drainRate * Time.deltaTime;

        if (bloodCapacity > drained) {

            bloodCapacity -= drained;
            return drained;

        }
        else {

            drained = bloodCapacity;
            bloodCapacity = 0;
            return drained;

        }

    }

}
