using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour {

    private List<Player> playersOnStairs;

    [SerializeField]
    private const string NOTIF_AREA = "notifArea";

    public const string LEVELS_NAME = "level";
    public const int NUM_LEVELS = 2;

    // Use this for initialization
    void Start() {
        // Debug.Log("start")  ;
        playersOnStairs = new List<Player>();
        displayNewLevel(getSceneIndex());
    }

    private void exitLevel() {
        // All players are on the stairs        
        int sceneIndex = getSceneIndex();
        sceneIndex++;
        if(sceneIndex > NUM_LEVELS) {
            FindObjectOfType<Notifier>().Notify(")=<    You lost    >=(");
        }
        else {
            string nextScene = LEVELS_NAME + sceneIndex;
            Debug.Log("Loading scene " + nextScene);
            SceneManager.LoadScene(nextScene);
        }
    }

    private int getSceneIndex() {
        string currentScene = SceneManager.GetActiveScene().name;
        // ASSUMES THAT the levels are named "level1" "level2.unity" etc.
        int sceneIndex;
        if (!int.TryParse(currentScene.Substring(LEVELS_NAME.Length), out sceneIndex)) {
            Debug.LogError("Could not parse level # from scene name " + currentScene);
        }
        return sceneIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player p = collision.gameObject.GetComponent<Player>();
        if (p != null) {
            playersOnStairs.Add(p);

            // Dead players will not be found here because their GOs are destroyed (replaced with corpses)
            Player[] livingPlayers = FindObjectsOfType<Player>();
            if (playersOnStairs.Count == livingPlayers.Length) {
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

    private void displayNewLevel(int levelIndex) {
        FindObjectOfType<Notifier>().Notify("Floor " + levelIndex);
    }
}
