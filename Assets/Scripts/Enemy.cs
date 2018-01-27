using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IShootable, IFreezable 
{
	private const float SPEED_MULTIPLIER = 0.01f;

    [SerializeField]
    private float speed = 10f;
	private float defaultSpeed;
	private float currentSpeed;
    [SerializeField]
    private float turnSpeed = 0.1f; // Percent of turn per FixedUpdate
    [SerializeField]
    private int health = 3;

	[SerializeField]
	private List<Vector2> path;
	private int nextPoint = 0;

	private Stack<Vector2> playerLocations;

	private Vector2 lastSeenPlayerLoc;
	private bool seePlayer = false;

	[SerializeField]
	private float spinSpeed = 20; // Degrees per FixedUpdate
	private bool spinning = false;
	private int spinUpdates;
	private int fullSpinUpdates;

	private bool turning = true;
	private Quaternion startRotation;
	private int turningUpdates;

    // Use this for initialization
    void Start()
	{
		transform.position = path [nextPoint];
		NextPathPoint ();

		playerLocations = new Stack<Vector2> ();

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
			seePlayer = true;
			spinning = false;
			turning = false;
			lastSeenPlayerLoc = (Vector2)foundPlayer.transform.position;
			Turn (lastSeenPlayerLoc);
			// #### Shoot at location ###
		}

		/* Stop shooting at player -- player hid */
		else if (foundPlayer == null && seePlayer) {
			playerLocations.Push (lastSeenPlayerLoc);
			seePlayer = false;
		}
			
		/* Performing a spin to find player */
		else if (spinning) {
			if (!Spin ()) {
				spinning = false;
				playerLocations.Pop ();
			}
		}

		/* Following last seen player location */
		else if (playerLocations.Count > 0) {
			if (!Move (playerLocations.Peek ())) {
				ToSpinState ();
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
	private bool Spin(){
		transform.Rotate (new Vector3(0,0, spinSpeed));
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