using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Actor, IShootable {
    [SerializeField]
    private KeyCode harvestKey;
    [SerializeField]
    private string playerID = "0";
    [SerializeField]
    private PlayerFollowCam followCam;

    private BloodPool bloodPool;

    private bool selected = false;

    private bool overwatching = false;
    private Quaternion overwatchRotation;

    // Use this for initialization
    protected override void AfterStart() {
        bloodPool = FindObjectOfType<BloodPool>();  
        if (playerID == "1") {
            Select();
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        // Select player
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)) {
            if (Input.GetKeyDown(playerID)) {
                Select();
            }
            else {
                Deselect();
            }
        }

        // Harvesting is checked first. You cannot take any other action while harvesting.
        if(Input.GetKey(harvestKey) && isHarvesting) {
            // Continue harvesting
            Harvest();
        }
        else if(isHarvesting) {
            // Stop harvesting
            StopHarvest();
        }
        else if(Input.GetKey(harvestKey)) {
            // Begin harvest
            Harvest();
        }
        else if (selected) {
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
        }
        else {
            // Overwatching
            if (overwatching) {
                Overwatch();
            }
        }
    }

    public bool IsSelected() {
        return selected;
    }

    void Select() {
        selected = true;
        followCam.setActivePlayer(gameObject.transform);
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
    }

    void Move(Vector3 direction) {
        transform.position += direction * speed * Time.deltaTime;

        EndOverwatch();
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
        GameObject visibleEnemy = LookForOpponent();
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
    
    protected override void Shoot() {
        Shoot(false);
    }

    protected override GameObject LookForOpponent() {
        return LookFor("Enemy", transform.up);
    }


    protected override float Harvest() {
        float amount = base.Harvest();
        bloodPool.Fill(amount);
        return amount;
    }
}