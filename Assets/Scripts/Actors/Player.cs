using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

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

    private static List<Player> livingPlayers = new List<Player>();

    // Use this for initialization
    protected override void Start() {
        base.Start();
        bloodPool = FindObjectOfType<BloodPool>();

        livingPlayers.Add(this);

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
        if(isHarvesting) {
            // Continue harvesting
            Harvest();
        }
        if (selected) {
            if (Input.GetKeyDown(harvestKey)) {
                if (isHarvesting) {
                    StopHarvest();
                }
                else {
                    Harvest();
                }
            }

            if (!isHarvesting) {
                // Face cursor
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 targetVector = mousePosition - transform.position;
                transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

                // Player Movement controls
				animator.SetBool("walk", false);
                if (Input.GetKey(KeyCode.W))
                {
                    Move(Vector3.up);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    Move(Vector3.down);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    Move(Vector3.left);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    Move(Vector3.right);
                }

                // Shooting controls
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Shoot();
                    EndOverwatch();
                }

                // Overwatch
                if (Input.GetKeyDown(KeyCode.F))
                {
                    StartOverwatch();
                }
            }
        }
        else {
            // Overwatching
            if (overwatching && !isHarvesting) {
                Overwatch();
            }
        }
    }

    private void OnMouseOver() {
        // Right-click a player to select them
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            Player[] players = FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Deselect();
            }
            Select();
        }
    }

    public bool IsSelected() {
        return selected;
    }

    public void Select() {
        selected = true;
        followCam.SetActivePlayer(gameObject.transform);
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
    }

    void Move(Vector3 direction) {
		animator.SetBool ("walk", true);

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

    protected override void Die() {
        base.Die();

        Deselect();

        int index = livingPlayers.IndexOf(this);
        livingPlayers.Remove(this);

        Debug.Log(livingPlayers.Count + " players remain1");
        if(livingPlayers.Count > 0) {
            StartCoroutine(cameraFollowNextPlayer());    
        }
        else {
            Debug.Log("You lost haha");
            GameObject.FindGameObjectWithTag("notifArea").GetComponent<Text>().text = "You lost haha";
        }
    }

    // Also destroys this object!! It should already be hidden.
    IEnumerator cameraFollowNextPlayer() {
        yield return new WaitForSeconds(1);

        Player newSelected = livingPlayers[0];
        Debug.Log("The new active player is " + newSelected.name);
        Debug.Log(livingPlayers.Count + " players remain");
        newSelected.Select();
        followCam.SetActivePlayer(newSelected.transform);

        Destroy(gameObject);
    }

    protected override GameObject LookForOpponent() {
        return LookFor("Enemy", transform.up);
    }

    protected override float Harvest() {
        if (bloodPool.IsFull()) {
            StopHarvest();
            return 0f;
        }
        float amount = base.Harvest();
        bloodPool.Fill(amount);
        return amount;
    }

    protected override float GetFireRate() {
        return 0.75f;
    }
}