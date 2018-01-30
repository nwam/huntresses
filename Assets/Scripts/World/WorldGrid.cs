using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class WorldGrid : MonoBehaviour {
    
    private List<GameObject> pathingObjects = new List<GameObject>();
    private const int numCols = 26;
    private const float cellOffset = 0.5f;
    private const float cellParam = 1f;
    
    public string[,] mapState;
    private Vector2[,] mapGrid = new Vector2[numCols,numCols];

    // Use this for initialization
    void Start() {
        // Initialize location grid
        int centering = numCols / 2;
        for (int i = 0; i < numCols; i++) {
            for (int j = 0; j < numCols; j++) {
                mapGrid[j, i] = new Vector2(cellOffset + cellParam * i - centering, cellOffset + cellParam * j - centering);
            }
        }
    }
    
    // Update is called once per frame
    void FixedUpdate() {

    }

    public string[,] getMapState() {
        string[,] refreshedMap = new string[numCols, numCols];
        for (int i = pathingObjects.Count - 1; i >= 0; i--) {
            if (pathingObjects[i] != null) { // Check if the object has been destroyed, and if so remove it
                Vector2 origPos = pathingObjects[i].transform.position;
                // Snap the position of the object to the grid
                Vector2 snapGridPos = new Vector2(Mathf.Round(origPos.x * 2.0f) / 2.0f, Mathf.Round(origPos.y * 2.0f) / 2.0f);

                // Find the indices of the position in the mapGrid, then map the object on the mapState
                for (int j = 0; j < numCols; j++) {
                    for (int k = 0; k < numCols; k++) {
                        if (mapGrid[k, j] == snapGridPos) {
                            refreshedMap[k, j] = pathingObjects[i].GetComponent<IPathLogic>().MapKey();
                        }
                    }
                }
            }
            else {
                pathingObjects.RemoveAt(i);
            }
        }
        return refreshedMap;
    }

    public void AddToMap(GameObject spawned) {
        pathingObjects.Add(spawned);
    }
}
