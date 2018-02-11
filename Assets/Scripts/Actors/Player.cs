using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

// These states are used by PlayerStatusDisplay to display how the player is currently acting.
// The player only has one state at a time. The initial state is Alive.
public enum PlayerState { ALIVE, HARVEST, BUBBLE, OVERWATCH, DEAD };

public class Player : Actor, IShootable, IPathLogic {

    [SerializeField]
    private KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField]
    private KeyCode harvestKey = KeyCode.Q;
    [SerializeField]
    private KeyCode overwatchKey = KeyCode.F;

    // Player has a worse FOV to keep a lid on Overwatch
    new protected int fov = 5;
    [SerializeField]
    private string playerID;
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
    private const float KEYPRESS_COOLDOWN = 0.1f;       // in seconds
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
        if(this != null) {
            this.state = state;
            statusDisplay.EnterState(state);
            // Debug.Log(name + " entering state " + state);
        }
        else {
            statusDisplay.EnterState(PlayerState.DEAD);
        }
    }

    private void RevertToBaseState() {
        EnterState(PlayerState.ALIVE);
    }

    public PlayerState GetState() {
        return state;
    }

    protected void Update() {
        base.FixedUpdate();

        if (harvestKeyPressTimer < KEYPRESS_COOLDOWN) {
            harvestKeyPressTimer += Time.deltaTime;
        }
        if (overwatchKeyPressTimer < KEYPRESS_COOLDOWN) {
            overwatchKeyPressTimer += Time.deltaTime;
        }

        // Select player
        // if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)) {
            // Select THIS player
            if (Input.GetKeyDown(playerID) && state != PlayerState.DEAD) {
                Select();
                Debug.Log("Select " + playerID);

                // Deselect other player
                livingPlayers
                        .Where(p => !p.Equals(this)).ToList()
                        .ForEach(p => p.Deselect());
            }
        // }

        // Harvesting is checked first. You cannot take any other action while harvesting.
        if (isHarvesting) {
            if (Input.GetKeyDown(harvestKey) && harvestKeyPressTimer >= KEYPRESS_COOLDOWN && IsSelected()) {
                Debug.Log("Requested leave harvest");
                StopHarvest();
            }
            else {
                Harvest();
            }
        }
        else if (state == PlayerState.OVERWATCH) {
            if (Input.GetKeyDown(overwatchKey) && overwatchKeyPressTimer >= KEYPRESS_COOLDOWN && IsSelected()) {
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
            if (Input.GetKey(shootKey)) {
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

    /*
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
    */

    public bool IsSelected() {
        return selected;
    }

    public void Select() {
        selected = true;
        followCam.SetActivePlayer(gameObject.transform);
        statusDisplay.SetSelected(selected);
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
        statusDisplay.SetSelected(selected);
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
        // Vector3 ray = Quaternion.Euler(0, 0, fov / 2) * transform.right * 10;

        // Find an enemy.
        GameObject visibleEnemy = LookForOpponent();
        if (visibleEnemy == null) {
            // Debug.Log("No visible enemy");
            return;
        }

        // Found an enemy. Rotate to point right at enemy.
        Vector3 targetVector = visibleEnemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, targetVector);

        Shoot();
    }

    /*
     * Tries to start the time bubble, and returns if the bubble was started so that TimeBubble class can continue.
     * Does not start the bubble if player is not selected.
     */
    public bool OnStartTimeBubble() {
        // Debug.Log("StartTimeBubble " + name);
        // Starting the bubble cancels Overwatch or Harvesting states
        if(IsSelected()) {
            EnterState(PlayerState.BUBBLE);
            return true;
        }
        return false;
    }

    public void OnStopTimeBubble() {
        // Debug.Log("StopTimeBubble " + name);
        if(state == PlayerState.BUBBLE) {
            RevertToBaseState();
        }
    }

    protected override void Shoot() {
        Shoot(false);
    }

    protected override void Die() {
        base.Die();

        // If the currently selected player dies, switch to the next living player
        livingPlayers.Remove(this);
        // Must determine if selected before setting state to dead
        // bool selected = IsSelected();
        EnterState(PlayerState.DEAD);

        Debug.Log(livingPlayers.Count + " players remain1");
        // Do not switch if the dead player was not selected
        if(IsSelected()) {
            //Debug.Log("A selected player died " + name);
            Deselect();
            StartCoroutine(cameraFollowNextPlayer());
        }
    }

    // TO be run on player death
    // Also checks for game over state!
    // Also destroys this object!! It should already be hidden.
    IEnumerator cameraFollowNextPlayer() {
        yield return new WaitForSeconds(1);

        if (livingPlayers.Count <= 0) {
            Debug.Log("You lost haha");
            FindObjectOfType<Notifier>().Notify("YOU LOST >=(", -1);
        }
        else {
            Player newSelected = livingPlayers[0];
            //Debug.Log("The new active player is " + newSelected.name);
            //Debug.Log(livingPlayers.Count + " players remain");
            newSelected.Select();
            followCam.SetActivePlayer(newSelected.transform);
        }

        Destroy(gameObject);
    }

    protected override GameObject LookForOpponent() {
        return LookFor("Enemy", transform.up, fov);
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
        return 1f;
    }

    public override float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public override string MapKey() {
        return "Player";
    }
}