using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour {

    [SerializeField]
    private float bloodCapacity = 10f;
    [SerializeField]
    private float drainRate = 1f;

    private IHarvester currentHarvester = null;

    private void OnTriggerEnter2D(Collider2D collision) {
        IHarvester newHarvester = collision.gameObject.GetComponent<IHarvester>();

        if (newHarvester != null) {
            newHarvester.AddHarvestTarget(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        IHarvester oldHarvester = collision.gameObject.GetComponent<IHarvester>();

        if (oldHarvester != null) {
            oldHarvester.RemoveHarvestTarget(this);
        }
    }

    public float Harvest(IHarvester harvester) {
        // Make sure nobody else is harvesting this corpse
        if (currentHarvester != null && currentHarvester != harvester) {
            return 0f;
        }

        // Mark this corpse as being harvested
        currentHarvester = harvester;

        float drained = drainRate * Time.deltaTime;

        if (bloodCapacity > drained) {
            bloodCapacity -= drained;
        }
        else {
            drained = bloodCapacity;
            bloodCapacity = 0;
        }
        return drained;
    }

    public void StopHarvest(IHarvester harvester) {
        if (harvester == currentHarvester) {
            currentHarvester = null;
        }
    }
}
