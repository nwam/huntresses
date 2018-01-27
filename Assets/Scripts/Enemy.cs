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
    private float turnSpeed = 0.1f;
    [SerializeField]
    private int health = 3;

	[SerializeField]
	private List<Vector2> path;
	private int nextPoint = 0;

	private Stack<Vector2> playerLocations;

	private bool turning = true;
	private Quaternion startRotation;
	private int turningUpdates;

    private Corpse harvestTarget, harvestingTarget;

    public GameObject corpsePrefab;

    // Use this for initialization
    void Start()
	{
		transform.position = path [nextPoint];
		NextPathPoint ();

		// Because we don't want all of our speeds to be 0.01, 0.015, etc.
		defaultSpeed = speed * SPEED_MULTIPLIER;
		currentSpeed = defaultSpeed;
    }

    private void FixedUpdate()
    {
		if (LookForPlayer ()) {
			print ("Found Player!");
		}

		if (!turning) {
			Move ();
		} else {
			Turn ();
		}
    }

	private bool LookForPlayer(){
		Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * transform.right.normalized);
		RaycastHit2D castHit = Physics2D.Raycast (frontOfSelf, transform.right);

		if (castHit.transform != null) {
			GameObject hitObject = castHit.transform.gameObject;

			if (hitObject.CompareTag("Player")){
				return true;
			}
		}
		return false;
	}

	private void Move(){        
		transform.position = Vector2.MoveTowards (transform.position, path [nextPoint], currentSpeed);

		if ((Vector2)transform.position == path [nextPoint]) {
			NextPathPoint ();
			ToTurnState ();
		}
	}

	private void Turn(){
		float turnProgress = turnSpeed * turningUpdates++;
		Vector3 targetDirection = path [nextPoint] - (Vector2)transform.position;
		float targetAngle = Mathf.Atan2 (targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
		Quaternion targetRotation = Quaternion.AngleAxis (targetAngle, Vector3.forward);
		transform.rotation = Quaternion.Slerp (startRotation, targetRotation, turnSpeed*turningUpdates);

		if (turnProgress >= 1) {
			ToMoveState ();
		}
	}

	private void ToTurnState(){
		turning = true;
		startRotation = transform.rotation;
		turningUpdates = 0;
	}

	private void ToMoveState(){
		turning = false;
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
			Debug.Log(name + " is dead");
            Die();
		}
    }

    private void Die()
    {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
        
        // TODO: Make the corpse use an enemy corpse sprite.

        Destroy(gameObject);
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