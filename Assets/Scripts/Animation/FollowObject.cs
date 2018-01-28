using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour	 {

	[SerializeField]
	private Vector3 offset;
	[SerializeField]
	private Transform target;

	private bool enabled = false;

	public void enable() {
		enabled = true;
	}

	public void disable() {
		enabled = false;
	}

	public void setOffset(Vector3 newOffset) {
		offset = newOffset;
	}

	public void setTarget(Transform newTarget) {
		target = newTarget;
	}

	void Update() {
		if (enabled && target != null) {
			transform.position = target.position + offset;
		}
	}
}
