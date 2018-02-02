using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IPathLogic {
    
	// Use this for initialization
	void Start () {
        OnSpawn();
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
    }

    public void OnSpawn() {
        WorldGrid.Instance.AddToMap(this.gameObject);
    }
}
