using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IShootable
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float health = 1f;
    [SerializeField]
    private bool selected = false;
    [SerializeField]
    private KeyCode harvestKey;

    public GameObject bulletPrefab;
    public GameObject corpsePrefab;
    //public TimeBubble timeBubble;

    private BloodPool bloodPool;

    public string playerID = "0";

    private Corpse harvestTarget, harvestingTarget; // harvestTarget is for proximity check, harvestingTarget is for no multiple drain check

    // Use this for initialization
    void Start()
    {
        if (playerID == "1")
        {
            Select();
        }
        bloodPool = FindObjectOfType<BloodPool>();
    }

    private void FixedUpdate()
    {
        // Select player
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (Input.GetKeyDown(playerID))
            {
                Select();
            }
            else
            {
                Deselect();
            }
        }

        if (selected)
        {
            // Face cursor
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetVector = mousePosition - transform.position;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

            //transform.LookAt(Input.mousePosition);

            // Player Movement controls
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
            }

            // Shooting controls
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }

            // Harvesting

            if (Input.GetKey(KeyCode.Q)) {
                if (harvestTarget == null) {
                    Debug.Log("I don't have a corpse to harvest!");
                }
                else if (!Harvest(harvestTarget)) {
                    Debug.Log("This corpse has no blood left to drain!");
                }
                else {
                    Debug.Log("This corpse has" + harvestTarget.getBloodCapacity() + " blood left");
                }
            }

        }
    }

    public bool isSelected() {
        return selected;
    }
    
    void Select() {
        selected = true;
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
    }

    void Shoot()
    {
        // Fire a bullet in the direction the player is facing
        GameObject newBulletGO = Instantiate(bulletPrefab, transform.position + transform.up * 0.8f, transform.rotation);
    }
    
    public void GetShot(int damage) {
        // Duped with Enemy but will diverge later (right?)
        // Re: above: probably won't? both should die and leave behind a corpse. Their Die() methods will differ once we get sprites in.
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            Debug.Log(name + " is dead");
            Die();
        }
    }

    private void Die()
    {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        // TODO: Make the corpse use the appropriate player corpse

        Destroy(gameObject);
    }

    private bool Harvest(Corpse corpse) {
        CircleCollider2D harvester = this.gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D harvestable = corpse.GetComponent<CircleCollider2D>();

        if (!corpse.getBeingHarvested() || corpse == harvestingTarget) // Check if already being harvested by someone else
        {
            harvestingTarget = corpse;
            float drained = corpse.beHarvested();
            bloodPool.Fill((int)drained); // TODO: Change blood pool to be a float?
            return drained != 0;
        }

        return false;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        Corpse swapHarvestTarget = collision.gameObject.GetComponent<Corpse>();

        if (swapHarvestTarget != null) { // Check if colliding with a corpse
            if (harvestTarget == null) { // If there is no current harvest target
                harvestTarget = swapHarvestTarget;
                Debug.Log("Set Harvest Target");
            }
            else if (swapHarvestTarget != harvestTarget) { // Swap target is not the current target
                Vector2 swapPos = collision.gameObject.transform.position;
                Vector2 curPos = harvestTarget.gameObject.transform.position;
                Vector2 thisPos = transform.position;

                if (Vector2.Distance(swapPos, thisPos) < Vector2.Distance(curPos, thisPos)) { // Set harvest target to closer corpse
                    harvestTarget = swapHarvestTarget;
                    Debug.Log("Swapped Harvest Target");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Corpse deselectTarget = collision.gameObject.GetComponent<Corpse>();

        if (deselectTarget != null && harvestTarget == deselectTarget) { // Check if exiting the current target corpse
            harvestTarget = null;
            Debug.Log("Lost Harvest Target");
        }
    }

}