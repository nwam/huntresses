using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IPathLogic {
    
	// Use this for initialization
	void Start () {
        onSpawn();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public string MapKey() {
        return "Wall";
        Debug.Log("I have been recorded");
    }

    public void onSpawn() {
        WorldGrid.AddToMap(this.gameObject);
    }
}
