using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float turnSpeed = 7.5f;
    [SerializeField]
    private int health = 3;
    [SerializeField]
    private float drainRate = 1f;
    private Corpse harvestTarget;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        bool turning = false;
        if (transform.position.y >= 5)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x, 5, 0), transform.rotation);
            transform.Rotate(transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (transform.rotation.z > 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 180));
            }
        }
        else if (transform.position.y <= -5)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x, -5, 0), transform.rotation);
            transform.Rotate(transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (transform.rotation.z < 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            }
        }

        if (!turning)
        {
            transform.position += speed * transform.up * Time.deltaTime;
        }
    }

    public bool Harvest(Corpse corpse) {

        CircleCollider2D harvester = this.gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D harvestable = corpse.GetComponent<CircleCollider2D>();

        if (harvester.IsTouching(harvestable) && (!corpse.getBeingHarvested() || corpse == harvestTarget)) {

            float drained = corpse.beHarvested();
            return drained != 0;

        }

        return false;

    }

}
