using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Enemies "forward" direction is position.right

public class Enemy : MonoBehaviour, IShootable, IFreezable 
{

	private struct PlayerLocation
	{
		public Vector2 location;
		public int direction;

		public PlayerLocation(Vector2 loc, int dir){
			direction = dir;
			location = loc;
		}
	}

	private const float SPEED_MULTIPLIER = 0.01f;

	[SerializeField]
	private GameObject projectilePrefab;

	[SerializeField]
	private int fireDelay = 30;
	private int fireDelayCount = 0;

	[SerializeField]
	private int health = 3;

    [SerializeField]
    private float speed = 10f;
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
    void Start()
	{
		transform.position = path [nextPoint];
		NextPathPoint ();

		playerLocations = new Stack<PlayerLocation> ();

		// Because we don't want all of our speeds to be 0.01, 0.015, etc.
		defaultSpeed = speed * SPEED_MULTIPLIER;
		currentSpeed = defaultSpeed;

		fullSpinUpdates = (int) (360 / spinSpeed);
    }

    private void FixedUpdate()
    {
		GameObject foundPlayer = LookForPlayer ();

		/* Shooting at player */
		if (foundPlayer != null) {

			// Keep track of the direction the player is heading in
			if (seePlayer == true) {
				deltaRot = Vector2.Angle (prevRight, transform.right);
			}
			prevRight = transform.right;
				
			// Stop all other actions
			seePlayer = true;
			spinning = false;
			turning = false;

			// Track the player's location
			lastSeenPlayerLoc = (Vector2)foundPlayer.transform.position;

			Turn (lastSeenPlayerLoc);

			if (fireDelayCount % fireDelay == 0) {
				Vector3 ea = transform.rotation.eulerAngles;
				Quaternion fireDirection = Quaternion.Euler (ea.x, ea.y, ea.z - 90);
				Instantiate (projectilePrefab, transform.position + transform.right * transform.lossyScale.x * 1.2f, fireDirection);
			}
			fireDelayCount += 1;
		}

		/* Stop shooting at player -- player hid... or died lol */
		else if (foundPlayer == null && seePlayer) {
			playerLocations.Push (new PlayerLocation (lastSeenPlayerLoc, (int)Mathf.Sign (-deltaRot)));
			lastPathLocation = transform.position;
			seePlayer = false;
			fireDelayCount = 0;
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
			if (!Move (playerLocations.Peek ().location)) {
				ToSpinState ();
			}
		}

		/* Returning to where we last left our patrol path */
		else if (returningToPath) {
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


	private GameObject LookForPlayer(){
		Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * transform.right.normalized);
		RaycastHit2D castHit = Physics2D.Raycast (frontOfSelf, transform.right);

		if (castHit.transform != null) {
			GameObject hitObject = castHit.transform.gameObject;

			if (hitObject.CompareTag("Player")){
				return hitObject;
			}
		}
		return null;
	}

	/* Returns false when enemy has reached destination */
	private bool Move(Vector2 destination){
		transform.position = Vector2.MoveTowards (transform.position, destination, currentSpeed);

		if ((Vector2)transform.position == destination) {
			return false;
		}
		return true;
	}

	private bool Turn(Vector2 destination){
		float turnProgress = turnSpeed * turningUpdates++;
		Vector3 targetDirection = destination - (Vector2)transform.position;
		float targetAngle = Mathf.Atan2 (targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
		Quaternion targetRotation = Quaternion.AngleAxis (targetAngle, Vector3.forward);
		transform.rotation = Quaternion.Slerp (startRotation, targetRotation, turnSpeed*turningUpdates);

		if (turnProgress >= 1) {
			return false;
		}

		return true;
	}

	// Returns true when still spinning
	private bool Spin(int direction){
		transform.Rotate (new Vector3(0,0, spinSpeed*direction));
		spinUpdates += 1;

		if (spinUpdates >= fullSpinUpdates) {
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


	public void GetShot(int damage) {
		health -= damage;
		Debug.Log(name + " got shot, now I have " + health + "hp");

		if (health <= 0) {
			// Die
			Debug.Log(name + " is dead");
		}
	}

	public void Freeze() {
		Debug.Log(name + " frozen");
		currentSpeed = 0;
		// Also cannot rotate or shoot
	}

	public void UnFreeze() {
		Debug.Log(name + " unfrozen");
		currentSpeed = defaultSpeed;
		// Restore ability to rotate and shoot
	}

	public bool isDestroyed() {
		return this == null;
	}
}