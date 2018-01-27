using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    Corpse harvestTarget;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public bool Harvest(Corpse corpse) {

        CircleCollider2D harvester = this.gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D harvestable = corpse.GetComponent<CircleCollider2D>();

        if (harvester.IsTouching(harvestable) && (!corpse.getBeingHarvested() || corpse == harvestTarget)) {

            float drained = corpse.beHarvested();
            // TODO: Fill blood pool
            return drained != 0;

        }

        return false;

    }

}
