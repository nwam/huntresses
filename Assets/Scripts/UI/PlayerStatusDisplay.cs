using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlayerStatusDisplay : MonoBehaviour {

    private Dictionary<PlayerState, Color> stateColors;

    private Image statusDisplayImg;

	// Use this for initialization
	void Start () {
        statusDisplayImg = GetComponent<Image>();

        stateColors = new Dictionary<PlayerState, Color>();
        stateColors.Add(PlayerState.ALIVE, Color.grey);
        stateColors.Add(PlayerState.BUBBLE, Color.green);
        stateColors.Add(PlayerState.HARVEST, Color.red);
        stateColors.Add(PlayerState.OVERWATCH, Color.blue);
        stateColors.Add(PlayerState.SELECTED, Color.white);
        // if StateColors.Count != PlayerState.getValues().Count, there is an error
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnterState(PlayerState state) {
        if(state == PlayerState.DEAD) {
            // Special state - Hide this StatusDisplay
            gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else {
            statusDisplayImg.color = stateColors[state];            
        }
    }
}
