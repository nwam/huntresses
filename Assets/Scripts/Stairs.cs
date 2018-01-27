using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Stairs : MonoBehaviour {

    private Player[] players;
    private List<Player> playersOnStairs;

    public const string LEVELS_NAME = "level";
    public const int NUM_LEVELS = 2;

    // Use this for initialization
    void Start() {
        players = FindObjectsOfType<Player>();
        playersOnStairs = new List<Player>();
    }

    // Update is called once per frame
    void Update() {
    }

    private void exitLevel() {
        // All players are on the stairs
        string currentScene = SceneManager.GetActiveScene().name;
        // ASSUMES THAT the levels are named "level1" "level2.unity" etc.
        int sceneIndex;
        if(!int.TryParse(currentScene.Substring(LEVELS_NAME.Length), out sceneIndex)) {
            Debug.LogError("Could not parse level # from scene name " + currentScene);
        }
        sceneIndex++;
        if(sceneIndex > NUM_LEVELS) {
            Debug.Log("Gratz you beat the game");
        }
        else {
            string nextScene = LEVELS_NAME + sceneIndex;
            Debug.Log("Loading scene " + nextScene);
            SceneManager.LoadScene(nextScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player p = collision.gameObject.GetComponent<Player>();
        if (p != null) {
            playersOnStairs.Add(p);

            if (playersOnStairs.Count == players.Length) {
                exitLevel();
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Player p = collision.gameObject.GetComponent<Player>();
        if (p != null) {
            playersOnStairs.Remove(p);
        }
    }
}
