using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Enemies "forward" direction is position.right

public class Enemy : MonoBehaviour, IShootable, IFreezable {

    private struct PlayerLocation {
        public Vector2 location;
        public int direction;

        public PlayerLocation(Vector2 loc, int dir) {
            direction = dir;
            location = loc;
        }
    }

    private const float SPEED_MULTIPLIER = 0.01f;

    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private int projectileCooldown = 30;
    private int projectileCooldownCount = 0;
    [SerializeField]
    private float fov = 60; // in degrees

    [SerializeField]
    private int health = 3;

    [SerializeField]
    private float speed = 10f;
	[SerializeField]
	private float chaseSpeed = 20f;
	private float defaultSpeed;
	private float currentSpeed;
    
	[SerializeField]
    private float turnSpeed = 0.1f; // Percent of turn per FixedUpdate
    private bool turning = true;
    private Quaternion startRotation;
    private int turningUpdates;

    [SerializeField]
    private float spinSpeed = 8; // Degrees per FixedUpdate
    private bool spinning = false;
    private int spinUpdates;
    private int fullSpinUpdates;

    [SerializeField]
    private List<Vector2> path;
    private int nextPoint = 0;
    
	[SerializeField]
	public GameObject corpsePrefab;
	private Corpse harvestTarget, harvestingTarget;

    private Stack<PlayerLocation> playerLocations;
    private Vector2 lastSeenPlayerLoc;
    private bool seePlayer = false;

    // Keep track of the direction which the player is heading in
    private float deltaRot = 0;
    private Vector2 prevRight;

    // When a player chase is over and the enemy wants to return
    // to where they left off
    private bool returningToPath = false;
    private Vector2 lastPathLocation;

    // Use this for initialization
    void Start() {
        transform.position = path[nextPoint];
        NextPathPoint();

        playerLocations = new Stack<PlayerLocation>();
        
		// Because we don't want all of our speeds to be 0.01, 0.015, etc.
		defaultSpeed = speed * SPEED_MULTIPLIER;
		chaseSpeed *= SPEED_MULTIPLIER;
		currentSpeed = defaultSpeed;

        fullSpinUpdates = (int)(360 / spinSpeed);
    }
    
    private void FixedUpdate()
    {
		GameObject foundPlayer = LookForPlayers ();

		/* Shooting at player */
		if (foundPlayer != null) {

			// Keep track of the direction the player is heading in
			if (seePlayer == true) {
				deltaRot = Vector2.SignedAngle (prevRight, transform.right);
			}
			prevRight = transform.right;
				
			// Stop all other actions
			seePlayer = true;
			spinning = false;
			turning = false;

			// Track the player's location
			lastSeenPlayerLoc = (Vector2)foundPlayer.transform.position;

			Turn (lastSeenPlayerLoc);

			if (projectileCooldownCount % projectileCooldown == 0) {
				Vector3 ea = transform.rotation.eulerAngles;
				Quaternion fireDirection = Quaternion.Euler (ea.x, ea.y, ea.z - 90);
				Instantiate (projectilePrefab, transform.position + transform.right * transform.lossyScale.x * 1.2f, fireDirection);
			}
			projectileCooldownCount += 1;
		}

		/* Stop shooting at player -- player hid... or died lol */
		else if (foundPlayer == null && seePlayer) {

			if (playerLocations.Count <= 0) {
				lastPathLocation = transform.position;
			}

			playerLocations.Push (new PlayerLocation (lastSeenPlayerLoc, (int)Mathf.Sign (deltaRot)));
			seePlayer = false;
			projectileCooldownCount = 0;
		}
			
		/* Performing a spin to find player */
		else if (spinning && playerLocations.Count > 0) {
			if (!Spin (playerLocations.Peek ().direction)) {
				spinning = false;
				playerLocations.Pop ();

				if (playerLocations.Count <= 0) {
					returningToPath = true;
				}
			}
		}

		/* Following last seen player location */
		else if (playerLocations.Count > 0) {
			if (currentSpeed != 0) {
				currentSpeed = chaseSpeed;
			}
			if (!Move (playerLocations.Peek ().location)) {
				ToSpinState ();
			}
		}

		/* Returning to where we last left our patrol path */
		else if (returningToPath) {
			if (currentSpeed != 0) {
				currentSpeed = defaultSpeed;
			}
			if (!Move (lastPathLocation)) {
				returningToPath = false;
			}
		}

		/* On preset path */
		else {
			if (!turning) {
				if (!Move (path [nextPoint])) {
					NextPathPoint ();
					ToTurnState ();
				}
			} else {
				if (!Turn (path [nextPoint])) {
					ToMoveState ();
				}
			}
		}
    }


    private GameObject LookForPlayers() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            float angle = Vector2.Angle(player.transform.position - transform.position, transform.right);

            if (Mathf.Abs(angle) <= fov / 2) {
                Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * transform.right.normalized);
                Vector2 rayDirection = player.transform.position - transform.position;
                RaycastHit2D castHit = Physics2D.Raycast(frontOfSelf, rayDirection);
                Debug.DrawRay(frontOfSelf, rayDirection);

                if (castHit.transform != null) {
                    GameObject hitObject = castHit.transform.gameObject;

                    if (hitObject.CompareTag("Player")) {
                        return hitObject;
                    }
                }
            }
        }


        return null;
    }

    /* Returns false when enemy has reached destination */
    private bool Move(Vector2 destination) {
        transform.position = Vector2.MoveTowards(transform.position, destination, currentSpeed);

        if ((Vector2)transform.position == destination) {
            return false;
        }
        return true;
    }

    private bool Turn(Vector2 destination) {
        float turnProgress = turnSpeed * turningUpdates++;
        Vector3 targetDirection = destination - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, turnSpeed * turningUpdates);

        if (turnProgress >= 1) {
            return false;
        }

        return true;
	}

	private void ToTurnState(){
		turning = true;
		startRotation = transform.rotation;
		turningUpdates = 0;
	}

	private void ToMoveState(){
		turning = false;
	}

	private void ToSpinState(){
		spinning = true;
		spinUpdates = 0;
	}

	private int NextPathPoint(){
		nextPoint += 1;
		nextPoint %= path.Count;
		return nextPoint;
	}

	public void Freeze() {
		Debug.Log(name + " frozen");
		currentSpeed = 0;
		// Also cannot rotate or shoot
	}


    // Returns true when still spinning
    private bool Spin(int direction) {
        transform.Rotate(new Vector3(0, 0, spinSpeed * direction));
        spinUpdates += 1;

        if (spinUpdates >= fullSpinUpdates) {
            return false;
        }
        return true;
    }

    public void UnFreeze() {
        Debug.Log(name + " unfrozen");
        currentSpeed = defaultSpeed;
        // Restore ability to rotate and shoot
    }

    public bool isDestroyed() {
        return this == null;
    }

    public void GetShot(int damage) {
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            Debug.Log(name + " is dead");
            Die();
        }
    }

    private void Die() {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        // TODO: Make the corpse use an enemy corpse sprite.

        Destroy(gameObject);
    }

    private bool Harvest(Corpse corpse) {
        CircleCollider2D harvester = this.gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D harvestable = corpse.GetComponent<CircleCollider2D>();

        if (!corpse.getBeingHarvested() || corpse == harvestingTarget) // Check if already being harvested by someone else
        {
            harvestingTarget = corpse;
            float drained = corpse.beHarvested();
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