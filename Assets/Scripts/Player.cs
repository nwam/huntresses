using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IShootable {
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float health = 1f;
    [SerializeField]
    private float fov = 60; // in degrees
    [SerializeField]
    private bool selected = false;
    [SerializeField]
    private KeyCode harvestKey;

    [SerializeField]
    private float timePerShot = 1f; // in seconds
    private float timeUntilShot = 0f;

    public GameObject bulletPrefab;
    public GameObject corpsePrefab;
    //public TimeBubble timeBubble;

    private BloodPool bloodPool;

    public string playerID = "0";

    private Corpse harvestTarget, harvestingTarget; // harvestTarget is for proximity check, harvestingTarget is for no multiple drain check
    private bool harvesting = false;

    private bool overwatching = false;
    private Quaternion overwatchRotation;

    // Use this for initialization
    void Start() {
        if (playerID == "1") {
            Select();
        }
        bloodPool = FindObjectOfType<BloodPool>();
    }

    private void FixedUpdate() {
        if (timeUntilShot > 0f)
        {
            timeUntilShot -= Time.deltaTime;
        }

        // Select player
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)) {
            if (Input.GetKeyDown(playerID)) {
                Select();
            }
            else {
                Deselect();
            }
        }

        if (selected) {
            // Face cursor
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetVector = mousePosition - transform.position;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

            //transform.LookAt(Input.mousePosition);

            // Player Movement controls
            if (Input.GetKey(KeyCode.W)) {
                Move(Vector3.up);
            }
            else if (Input.GetKey(KeyCode.S)) {
                Move(Vector3.down);
            }
            if (Input.GetKey(KeyCode.A)) {
                Move(Vector3.left);
            }
            else if (Input.GetKey(KeyCode.D)) {
                Move(Vector3.right);
            }

            // Shooting controls
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Shoot();
                EndOverwatch();
            }

            // Overwatch
            if (Input.GetKeyDown(KeyCode.F)) {
                StartOverwatch();
            }

            // Harvesting Toggle

            if (Input.GetKeyDown(KeyCode.Q)) {
                if (!harvesting) {
                    if (harvestTarget == null) {
                        Debug.Log("I don't have a corpse to harvest!");
                    }
                    else {
                        harvesting = true;
                    }
                }
                else {
                    harvesting = false;
                    harvestingTarget.setBeingHarvested(false);
                    harvestingTarget = null;
                }
            }
        }
        else // Not selected
        {
            // Overwatching
            if (overwatching)
            {
                Overwatch();
            }
        }

        // Harvesting

        if (harvesting) {
            if (!Harvest(harvestTarget)) {
                Debug.Log("This corpse has no blood left to drain!");
                harvesting = false;
                harvestingTarget.setBeingHarvested(false);
                harvestingTarget = null; // Reset harvesting target
            }
            else {
                Debug.Log("This corpse has " + harvestingTarget.getBloodCapacity() + " blood left.");
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

    void Move(Vector3 direction) {
        transform.position += direction * speed * Time.deltaTime;

        EndOverwatch();
    }

    void Shoot() {
        if (timeUntilShot <= 0f)
        {
            // Fire a bullet in the direction the player is facing
            GameObject newBulletGO = Instantiate(bulletPrefab, transform.position + transform.up * 0.8f, transform.rotation);
            timeUntilShot = timePerShot;
        }
    }

    void StartOverwatch() {
        overwatching = true;
        overwatchRotation = transform.rotation;
        Debug.Log("Player " + playerID + " overwatching");
    }

    void EndOverwatch() {
        overwatching = false;
    }

    void Overwatch() {
        transform.rotation = overwatchRotation;

        // Find an enemy.
        GameObject visibleEnemy = LookForEnemy();
        if (visibleEnemy == null) {
            Debug.Log("No visible enemy");
            return;
        }

        // Found an enemy. Rotate to point right at enemy.
        Vector3 targetVector = visibleEnemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, targetVector);

        // FIRE ZE ARROWS!!!1!
        Shoot();

    }
    
    private GameObject LookForEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float angle = Vector2.Angle(enemy.transform.position - transform.position, transform.up);

            if (Mathf.Abs(angle) <= fov / 2)
            {
                Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * transform.up.normalized);
                Vector2 rayDirection = enemy.transform.position - transform.position;
                RaycastHit2D castHit = Physics2D.Raycast(frontOfSelf, rayDirection);
                Debug.DrawRay(frontOfSelf, rayDirection);

                if (castHit.transform != null)
                {
                    GameObject hitObject = castHit.transform.gameObject;

                    if (hitObject.CompareTag("Enemy"))
                    {
                        return hitObject;
                    }
                }
            }
        }
        
        return null;
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

    private void Die() {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        // TODO: Make the corpse use the appropriate player corpse

        Destroy(gameObject);
    }

    private bool Harvest(Corpse corpse) {
        if (!corpse.getBeingHarvested() || corpse == harvestingTarget) // Check if already being harvested by someone else
        {
            harvestingTarget = corpse;
            float drained = corpse.beHarvested();
            bloodPool.Fill(drained); // TODO: Change blood pool to be a float?
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