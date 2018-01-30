using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class WorldGrid : MonoBehaviour {
    
    private static List<GameObject> pathingObjects = new List<GameObject>();
    private const int numCols = 26;
    private const float cellOffset = 0.5f;
    private const float cellParam = 1f;
    private const float centering = (numCols / 2) * cellParam;
    
    private static string[,] mapState;
    public static Vector2[,] mapGrid = new Vector2[numCols,numCols];

    // Use this for initialization
    void Awake() {
        Debug.Log("WorldGrid is awake!");

        // Initialize location grid
        string mapGridString = "";
        for (int i = 0; i < numCols; i++) {
            mapGridString += "[";
            for (int j = 0; j < numCols; j++) {
                mapGrid[j, i] = new Vector2(cellOffset + cellParam * j - centering, cellOffset + cellParam * i - centering);
                mapGridString += mapGrid[j, i].ToString() + ", ";
            }
            mapGridString += "]\n";
        }
        //Debug.Log(mapGridString);
    }
    
    // Update is called once per frame
    void FixedUpdate() {

    }

    public static string[,] getMapState() {
        Debug.Log("Getting map state. pathingObjects: " + pathingObjects.Count);
        string[,] refreshedMap = new string[numCols, numCols];
        for (int i = pathingObjects.Count - 1; i >= 0; i--) {
            if (pathingObjects[i] != null) { // Check if the object has been destroyed, and if so remove it
                Vector2 origPos = pathingObjects[i].transform.position;
                // Snap the position of the object to the grid
                Vector2 snapGridPos = new Vector2(Mathf.Round(origPos.x * 2.0f) / 2.0f, Mathf.Round(origPos.y * 2.0f) / 2.0f);

                int xIndex = ToIndex(snapGridPos.x);
                int yIndex = ToIndex(snapGridPos.y);

                refreshedMap[xIndex, yIndex] = pathingObjects[i].GetComponent<IPathLogic>().MapKey();
            }
        }
        return refreshedMap;
    }

    // When an IPathLogic object is spawned, it should be added to the list
    public static void AddToMap(GameObject spawned) {
        pathingObjects.Add(spawned);
        Debug.Log("Added to map: " + spawned);
    }

    public static int ToIndex(float pos) {
        int index = (int)((pos + centering - cellOffset) / cellParam);
        return index;
    }
}
