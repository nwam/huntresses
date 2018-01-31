using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlayerStatusDisplay : MonoBehaviour {

    private Dictionary<PlayerState, Color> stateColors;

    private Image statusDisplayImg;
    [SerializeField]
    private GameObject isSelectedImg;

	// Use this for initialization
	void Start () {
        statusDisplayImg = GetComponent<Image>();

        stateColors = new Dictionary<PlayerState, Color>();
        stateColors.Add(PlayerState.ALIVE, Color.white);
        stateColors.Add(PlayerState.BUBBLE, Color.green);
        stateColors.Add(PlayerState.HARVEST, Color.red);
        stateColors.Add(PlayerState.OVERWATCH, Color.blue);
        // stateColors.Add(PlayerState.SELECTED, Color.white);
        // if StateColors.Count != PlayerState.getValues().Count, there is an error
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnterState(PlayerState state) {
        if(stateColors == null) {
            Start();
        }

        if(state == PlayerState.DEAD) {
            // Special state - Hide this StatusDisplay
            // Set size to 0 to hide it
            gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else {
            statusDisplayImg.color = stateColors[state];            
        }
    }

    // Disable or enable the IsSelected indicator
    public void SetSelected(bool selected) {
        if(selected) {
            isSelectedImg.transform.localScale = new Vector3(1, 1, 1);
        }
        else {
            // Set size to 0 to hide it
            isSelectedImg.transform.localScale = new Vector3(0, 0, 1);
        }
    }
}
