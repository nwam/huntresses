using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IPathLogic {

    [SerializeField]
    private GameObject worldGrid;
    private GameObject spawnSender;

	// Use this for initialization
	void Start () {
        spawnSender = Instantiate(worldGrid) as GameObject;
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
    }

    public void onSpawn() {
        spawnSender.SendMessage("AddToMap", this.gameObject, SendMessageOptions.RequireReceiver);
    }
}
