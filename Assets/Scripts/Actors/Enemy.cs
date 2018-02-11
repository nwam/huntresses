using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Enemies "forward" direction is position.right

[RequireComponent(typeof(Animator))]
public class Enemy : Actor, IFreezable, IPathLogic {

    public struct PlayerLocation {
        public Vector2 location;
        public int direction;

        public PlayerLocation(Vector2 loc, int dir) {
            direction = dir;
            location = loc;
        }
    }

	private enum State{PATROL, COMBAT, SEARCH, SPIN, RETURN};
    private State state;

    private const float SPEED_MULTIPLIER = 0.01f;
    
    [SerializeField]
    private float chaseSpeed = 20f;
    private float defaultSpeed;
    private float currentSpeed;

    [SerializeField]
    private float turnSpeed = 8.0f;     // Degrees per FixedUpdate
    private bool turning = true;

    [SerializeField]
    private float spinSpeed = 8;        // Degrees per FixedUpdate
    private bool spinning = false;
    private int spinUpdates;
    private int fullSpinUpdates;
    private int spinDirection;

    [SerializeField]
    private List<Vector2> path;
    private int nextPoint = 0;

    private Stack<PlayerLocation> playerLocations;
    private Vector2 lastSeenPlayerLoc;
    private bool seePlayer = false;

    private Stack<Vector2> pathToReturnToPatrol;

    // Keep track of the direction which the player is heading in
    private float deltaRot = 0;
    private Vector2 prevRight;

    // When a player chase is over and the enemy wants to return
    // to where they left off
    private bool returningToPath = false;

    [SerializeField]
    private float fireRate;
	private bool isFrozen = false;

    
    // Use this for initialization
    protected override void Start() {
        base.Start();

        if(path.Count == 0) {
            // Debug.LogError("Need to set a path for enemy " + name);
        }
        transform.position = path[nextPoint];
        NextPathPoint();

        playerLocations = new Stack<PlayerLocation>();

        // Because we don't want all of our speeds to be 0.01, 0.015, etc.
        defaultSpeed = speed * SPEED_MULTIPLIER;
        chaseSpeed *= SPEED_MULTIPLIER;
        currentSpeed = defaultSpeed;

        fullSpinUpdates = (int)(360 / spinSpeed);

        if (enemyType == EnemyType.Normal) {
            health = 1;
        }
        else if (enemyType == EnemyType.Large) {
            health = 3;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
		animator.SetBool("walk", false);
        GameObject foundPlayer = LookForOpponent();

        /* COMBAT */
        if (foundPlayer != null) {
            // Debug.Log("COMBAT");

            // Keep track of the direction the player is heading in
            if (seePlayer == true) {
                deltaRot = Vector2.SignedAngle(prevRight, transform.right);
            }
            prevRight = transform.right;

            // Stop all other actions
            seePlayer = true;
            spinning = false;
            turning = false;

            // Track the player's location
            lastSeenPlayerLoc = (Vector2)foundPlayer.transform.position;
            // Debug.Log(" player at " + lastSeenPlayerLoc);

            Turn(lastSeenPlayerLoc);
            // Debug.Log("Finished turning");

            Shoot();
            // Debug.Log("Finished shooting");
        }

        /* Stop shooting at player -- player hid... or died lol */
        else if (foundPlayer == null && seePlayer) {
            // Debug.Log("Stop shooting");
            ChasePlayer(new PlayerLocation(lastSeenPlayerLoc, (int)Mathf.Sign(deltaRot)));
            seePlayer = false;
        }

        /* SPIN -- I actually do think it needs to be a state */
        else if (spinning) {
            // Debug.Log("SPIN");
            if (!Spin(spinDirection)) {
                ToReturnState();
            }
        }

        /* SEARCH */
        else if (playerLocations.Count > 0) {
            // Debug.Log("SEARCHing to " + playerLocations.Peek().location);
            if (currentSpeed != 0) {
                currentSpeed = chaseSpeed;
            }
            if (!Move(playerLocations.Peek().location)) {
                ToSpinState();
            }
        }

        /* RETURN */
        else if (returningToPath) {
            if (currentSpeed != 0) {
                currentSpeed = defaultSpeed;
            }
            // print("RETURNing to " + pathToReturnToPatrol.Peek());
            if (!Move(pathToReturnToPatrol.Peek())) {
                // Made it back to patrol path
                pathToReturnToPatrol.Pop();
                NextPathPoint();

                if (pathToReturnToPatrol.Count <= 0) {
                    returningToPath = false;
                }
            }
        }

        /* PATROL */
        else {
            // Debug.Log("PATROLing to " + path[nextPoint]);
            if (path.Count > 1 && !Move(path[nextPoint])) {
                NextPathPoint();
            }
        }
    }
    
    /* Returns false when enemy has reached destination */
    private bool Move(Vector2 destination) {
		animator.SetBool("walk", true);
        // Turn to look at destination
        // Vector2 currentPosition2D = transform.position;
        // Vector2 rotToDest = destination - currentPosition2D;
        //transform.up = Vector2.Lerp(position2, rot, Time.deltaTime * turnSpeed);
        // Debug.Log("Moving");

        if(Turn(destination)) {
            // Don't move
            // Debug.Log("Not moving cause turned");
            return true;
        }

        // Move towards destination
        transform.position = Vector2.MoveTowards(transform.position, destination, currentSpeed);

        if ((Vector2)transform.position == destination) {
            // Debug.Log("Reached move location of " + destination);
            return false;
        }

        // Debug.Log("Still moving to " + destination);
        return true;
    }

    /* Returns true of the turn executed successfully */
    private bool Turn(Vector2 destination) {

        float remainingRotation = Vector2.SignedAngle(transform.right.normalized, (destination - (Vector2)transform.position).normalized);

        // print("REMAINING ROTATION" + remainingRotation);
        if (turnSpeed >= Mathf.Abs(remainingRotation)) {
            transform.Rotate(new Vector3(0,0,remainingRotation));
            return false;
        }

        transform.Rotate(new Vector3(0,0,turnSpeed*Mathf.Sign(remainingRotation)));
        return true;
    }

    private void ChasePlayer(PlayerLocation playerLoc) {
        playerLocations.Push(playerLoc);
    }

    private void ToSpinState() {
        spinning = true;
        spinUpdates = 0;
        spinDirection = playerLocations.Peek().direction;

        // TODO: player locations should only be one location
        playerLocations = new Stack<PlayerLocation>();
    }

    private void ToReturnState() {
        spinning = false;
        returningToPath = true;
        pathToReturnToPatrol = new Stack<Vector2>(WorldGrid.Instance.AStar(transform.position, path[nextPoint]));
    }

    private int NextPathPoint() {
        nextPoint += 1;
        nextPoint %= path.Count;
        return nextPoint;
    }

    // Returns true when still spinning
    private bool Spin(int direction) {
        transform.Rotate(new Vector3(0, 0, spinSpeed * direction));
        spinUpdates += 1;

        if (spinUpdates >= fullSpinUpdates) {
			spinUpdates = 0;
            return false;
        }
        return true;
    }

    protected override void Shoot() {
		if (isFrozen) {
			// You cannot shoot while frozen.
			return;
		}
        Shoot(true);
    }

    public void Freeze() {
        // Debug.Log(name + " frozen");
		isFrozen = true;
        currentSpeed = 0;
        // Also cannot rotate or shoot
    }

    public void UnFreeze() {
        // Debug.Log(name + " unfrozen");
		isFrozen = false;
        currentSpeed = defaultSpeed;
        // Restore ability to rotate and shoot
    }

    public bool IsDestroyed() {
        return this == null;
    }

    protected override GameObject LookForOpponent() {
        return LookFor("Player", transform.right, fov);
    }

    // The cooldown in between shots, IE 1 / FireRate = Shots / second
    protected override float GetFireRate() {
        return fireRate;
    }

    protected override void Die() {
        base.Die();
        Destroy(gameObject);
    }

    public void HearNoise(PlayerLocation loc) {
        // Debug.Log("Heared a noise at " + loc.location);
        //transform.LookAt(loc.location);
        ChasePlayer(loc);
    }

    public override float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public override string MapKey() {
        return "Enemy";
    }
}