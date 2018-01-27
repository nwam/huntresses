using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowCam : MonoBehaviour {

    [SerializeField]
    private Vector3 offset;

    private Transform activePlayer;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        setPositionRelativeToPlayer();
	}

    public void setActivePlayer(Transform player) {
        activePlayer = player;
        setPositionRelativeToPlayer();
    }

    private void setPositionRelativeToPlayer() {
        transform.position = activePlayer.transform.position + offset;		
    }
}
