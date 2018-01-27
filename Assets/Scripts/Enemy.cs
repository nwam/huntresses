using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	private const float SPEED_MULTIPLIER = 0.01f;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float turnSpeed = 0.1f;
    [SerializeField]
    private int health = 3;
	[SerializeField]
	private List<Vector2> path;
	private int nextPoint = 0;

	private bool turning = true;
	private Quaternion startRotation;
	private int turningUpdates;

    // Use this for initialization
    void Start()
	{
		transform.position = path [nextPoint];
		NextPathPoint ();

		// Because we don't want all of our speeds to be 0.01, 0.015, etc.
		speed *= SPEED_MULTIPLIER;
    }

    private void FixedUpdate()
    {
		if (!turning) {
			Move ();
		} else {
			Turn ();
		}
    }


	private void Move(){
		transform.position = Vector2.MoveTowards (transform.position, path [nextPoint], speed);

		if ((Vector2)transform.position == path [nextPoint]) {
			NextPathPoint ();
			ToTurnState ();
		}
	}

	private void Turn(){
		Vector3 targetDirection = path [nextPoint] - (Vector2)transform.position;
		float targetAngle = Mathf.Atan2 (targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
		Quaternion targetRotation = Quaternion.AngleAxis (targetAngle, Vector3.forward);
		float turnProgress = turnSpeed * turningUpdates;
		transform.rotation = Quaternion.Slerp (startRotation, targetRotation, turnSpeed*turningUpdates);
		turningUpdates += 1;

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
}
