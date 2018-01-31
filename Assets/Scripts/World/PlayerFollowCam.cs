using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerFollowCam : MonoBehaviour {

    [SerializeField]
    private Vector3Int offset;

    // The level Height and Width 
    [SerializeField]
    private int levelWidth = -1;
    [SerializeField]
    private int levelHeight = -1;

    private Camera cam;

    private Transform activePlayer;

	// Use this for initialization
	void Start () {
        if(levelWidth < 0) {
            Debug.LogError("Invalid level width " + levelWidth);
        }
        if(levelHeight < 0) {
            Debug.LogError("Invalid level height " + levelHeight);
        }

        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if(activePlayer != null) {
            SetPositionRelativeToPlayer();
            ConstrictToLevelBounds();
        }
	}

    public void SetActivePlayer(Transform player) {
        activePlayer = player;
        SetPositionRelativeToPlayer();
    }

    private void SetPositionRelativeToPlayer() {
        transform.position = activePlayer.transform.position + offset;		
    }

    private void ConstrictToLevelBounds() {
        // Make sure the camera does not show any empty space outside the level.

        float halfViewportWidth = -offset.z;

        // Left boundary
        int boundary = Mathf.RoundToInt(-levelWidth / 2);
        float viewportEdge = transform.position.x - halfViewportWidth;
        if (viewportEdge < boundary) {
            float diff = viewportEdge - boundary;
            transform.position = new Vector3(transform.position.x - diff, transform.position.y, transform.position.z);
        }
        // Right
        else {
            boundary *= -1;
            viewportEdge = transform.position.x + halfViewportWidth;
            if (viewportEdge > boundary) {
                float diff = viewportEdge - boundary;
                transform.position = 
                    new Vector3(transform.position.x - diff, transform.position.y, transform.position.z);
            }
        }

        // Adjust the viewport width for y axis 
        halfViewportWidth *= 1/cam.aspect;
        // Bottom
        boundary = Mathf.RoundToInt(-levelHeight / 2);
        viewportEdge = transform.position.y - halfViewportWidth;
        if (viewportEdge < boundary) {
            float diff = viewportEdge - boundary;
            transform.position = new Vector3(transform.position.x, transform.position.y - diff, transform.position.z);
        }
        // Top
        else {
            boundary *= -1;
            viewportEdge = transform.position.y + halfViewportWidth;
            if (viewportEdge > boundary) {
                float diff = viewportEdge - boundary;
                transform.position = 
                    new Vector3(transform.position.x, transform.position.y - diff, transform.position.z);
            }
        }
    }
}
