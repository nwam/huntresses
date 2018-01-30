using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

// These states are used by PlayerStatusDisplay to display how the player is currently acting.
// The player only has one state at a time. The initial state is Alive.
public enum PlayerState { ALIVE, SELECTED, HARVEST, BUBBLE, OVERWATCH, DEAD };

public class Player : Actor, IShootable {
    [SerializeField]
    private KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField]
    private KeyCode harvestKey = KeyCode.Q;
    [SerializeField]
    private KeyCode overwatchKey = KeyCode.F;
    [SerializeField]
    private string playerID = "0";
    [SerializeField]
    private PlayerFollowCam followCam;
    [SerializeField]
    private PlayerStatusDisplay statusDisplay;

    private static List<Player> livingPlayers = new List<Player>();

    // The player state should only ever be modified by methods RevertToBaseStart and EnterState
    private PlayerState state;

    private BloodPool bloodPool;

    private bool selected = false;

    // These variables prevent the 'same' keyDown event from being processed twice because the player held it down for 
    private const float KEYPRESS_COOLDOWN = 0.2f;       // in seconds
    private float overwatchKeyPressTimer = KEYPRESS_COOLDOWN;
    private float harvestKeyPressTimer = KEYPRESS_COOLDOWN;

    // Use this for initialization
    protected override void Start() {
        base.Start();
        bloodPool = FindObjectOfType<BloodPool>();

        livingPlayers.Add(this);
        EnterState(PlayerState.ALIVE);

        if (playerID == "1") {
            Select();
        }
    }

    private void EnterState(PlayerState state) {
        this.state = state;
        statusDisplay.EnterState(state);
        Debug.Log(name + " entering state " + state);
    }

    private void RevertToBaseState() {
        if (IsSelected()) {
            EnterState(PlayerState.SELECTED);
        }
        else {
            EnterState(PlayerState.ALIVE);
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (harvestKeyPressTimer < KEYPRESS_COOLDOWN) {
            harvestKeyPressTimer += Time.deltaTime;
        }
        if (overwatchKeyPressTimer < KEYPRESS_COOLDOWN) {
            overwatchKeyPressTimer += Time.deltaTime;
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

        // Harvesting is checked first. You cannot take any other action while harvesting.
        if (isHarvesting) {
            if (Input.GetKeyDown(harvestKey) && harvestKeyPressTimer >= KEYPRESS_COOLDOWN) {
                Debug.Log("Requested leave harvest");
                StopHarvest();
            }
            else {
                Harvest();
            }
        }
        else if (state == PlayerState.OVERWATCH) {
            if (Input.GetKeyDown(overwatchKey) && overwatchKeyPressTimer >= KEYPRESS_COOLDOWN) {
                Debug.Log("Requested leave overwatch");
                EndOverwatch();
            }
            else {
                Overwatch();
            }
        }
        else if (selected) {
            // Face cursor
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetVector = mousePosition - transform.position;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

            // Player Movement controls
            animator.SetBool("walk", false);
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
            if (Input.GetKeyDown(shootKey)) {
                Shoot();
            }
            // Succ
            if(Input.GetKeyDown(harvestKey)) {
                Harvest();
            }
            // Overwatch
            if (Input.GetKeyDown(overwatchKey)) {
                StartOverwatch();
            }
        }
    }

    private void OnMouseOver() {
        // Right-click a player to select them
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            Player[] players = FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++) {
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
        followCam.setActivePlayer(gameObject.transform);
        if(state == PlayerState.ALIVE) {
            EnterState(PlayerState.SELECTED);
        }
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
        if(state == PlayerState.SELECTED) {
            EnterState(PlayerState.ALIVE);
        }
    }

    void Move(Vector3 direction) {
        animator.SetBool("walk", true);

        transform.position += direction * speed * Time.deltaTime;

        EndOverwatch();
    }

    void StartOverwatch() {
        EnterState(PlayerState.OVERWATCH);
        Debug.Log("Player " + playerID + " overwatching");
    }

    void EndOverwatch() {
        if (state == PlayerState.OVERWATCH) {
            RevertToBaseState();
        } 
    }

    void Overwatch() {
        // Find an enemy.
        GameObject visibleEnemy = LookForOpponent();
        if (visibleEnemy == null) {
            Debug.Log("No visible enemy");
            return;
        }

        // Found an enemy. Rotate to point right at enemy.
        Vector3 targetVector = visibleEnemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, targetVector);

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
        if (livingPlayers.Count > 0) {
            StartCoroutine(cameraFollowNextPlayer());
        }
        else {
            Debug.Log("You lost haha");
            GameObject.FindGameObjectWithTag("notifArea").GetComponent<Text>().text = "You lost haha";
        }
        EnterState(PlayerState.DEAD);
    }

    // Also destroys this object!! It should already be hidden.
    IEnumerator cameraFollowNextPlayer() {
        yield return new WaitForSeconds(1);

        Player newSelected = livingPlayers[0];
        Debug.Log("The new active player is " + newSelected.name);
        Debug.Log(livingPlayers.Count + " players remain");
        newSelected.Select();
        followCam.setActivePlayer(newSelected.transform);

        Destroy(gameObject);
    }

    protected override GameObject LookForOpponent() {
        return LookFor("Enemy", transform.up);
    }

    protected override float Harvest() {
        EnterState(PlayerState.HARVEST);
        float amount = base.Harvest();
        float diff = bloodPool.Fill(amount);
        if(diff == 0) {
            StopHarvest();
            Debug.Log("BP is full, stopping harvest");
            return 0f;
        }
        return amount;
    }

    protected override void StopHarvest() {
        base.StopHarvest();
        RevertToBaseState();
    }

    protected override float GetFireRate() {
        return 0.75f;
    }
}