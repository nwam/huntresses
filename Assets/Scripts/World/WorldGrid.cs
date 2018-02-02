using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class WorldGrid : Singleton<WorldGrid> {

    private List<GameObject> pathingObjects = new List<GameObject>();
    private const int numCols = 26;
    private const float cellOffset = 0.5f;
    private const float cellParam = 1f;
    private const float centering = (numCols / 2) * cellParam;

    private static string[,] mapState;
    public static Vector2[,] mapGrid = new Vector2[numCols, numCols];

    /* Would be nice to refactor to use these
    ... but that's only a dream :''''(
    private struct GridTile{
        public int x;
        public int y;

        public GridTile(int x, int y) {
            this.x = x;
            this.y = y;
        }

    }

    private struct WorldTile{
        public float x;
        public float y;

        public WorldTile(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public WorldTile(Vector2 v) {
            this.x = v.x;
            this.y = v.y;
        }

    }
    */

    // Use this for initialization
    void Awake() {
        for (int i = 0; i < numCols; i++) {
            for (int j = 0; j < numCols; j++) {
                mapGrid[j, i] = new Vector2(cellOffset + cellParam * j - centering, cellOffset + cellParam * i - centering);
            }
        }
    } 

    /* TODO: make this change mapState inplace instead of creating a new map */
    public string[,] updateMapState() {

        string[,] refreshedMap = new string[numCols, numCols];

        for (int i = pathingObjects.Count - 1; i >= 0; i--) {
            if (pathingObjects[i] != null) { // Check if the object has been destroyed, and if so remove it
                Vector2 worldPos = pathingObjects[i].transform.position;
                Vector2 gridPos = WorldToGrid(worldPos);

                int xIndex = (int)gridPos.x;
                int yIndex = (int)gridPos.y;

                refreshedMap[xIndex, yIndex] = pathingObjects[i].GetComponent<IPathLogic>().MapKey();
            }
        }

        mapState = refreshedMap;
        return mapState;
    }

    // When an IPathLogic object is spawned, it should be added to the list
    public void AddToMap(GameObject spawned) {
        pathingObjects.Add(spawned);
    }

    public static int ToIndex(float pos) {
        int index = (int)((pos + centering - cellOffset) / cellParam);
        return index;
    }

    public static Vector2 SnapToWorldTile(Vector2 world) {
        return new Vector2(Mathf.Round(world.x * 2.0f) / 2.0f, Mathf.Round(world.y * 2.0f) / 2.0f);
    }
       
    public static Vector2 WorldToGrid(Vector2 world) {
        world = SnapToWorldTile(world);
        return new Vector2(ToIndex(world.x), ToIndex(world.y));
    }

    public static Vector2 GridToWorld(Vector2 grid) {
        return mapGrid[(int)grid.x, (int)grid.y];
    }

    /* Returns path from start to goal
     *  Start, goal, and path are all in world coordinates
     */
    public List<Vector2> AStar(Vector2 start, Vector2 goal) {

        start = WorldToGrid(start);
        goal = WorldToGrid(goal);

        //print("Performing AStar to go from " + start + " to " + goal);

        updateMapState();
        //Print2DArray(mapState);

        Dictionary<Vector2, HashSet<Vector2>> adjList = GetAdjacencyList();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        List<Vector2> fringe = new List<Vector2>();
        Vector2[,] cameFrom = new Vector2[mapState.GetLength(0), mapState.GetLength(1)];
        
        // Cost of getting from start to node
        float[,] gScore = new float[mapState.GetLength(0), mapState.GetLength(1)];
        gScore = Populate2D<float>(gScore, Mathf.Infinity);
        gScore[(int)start.x, (int)start.y] = 0;

        // Estimated cost to goal
        float[,] fScore = new float[mapState.GetLength(0), mapState.GetLength(1)];
        fScore = Populate2D<float>(fScore, Mathf.Infinity);

        // Add start node
        float startScore = ManhattanDistance(start, goal);
        fScore[(int)start.x, (int)start.y] = startScore;
        fringe.Add(start);

        // Look at nodes that we haven't seen
        while (fringe.Count > 0) {
            // Look at node with closest heuristic
            float minFScore = Mathf.Infinity;
            Vector2 current = fringe[0];

            foreach (Vector2 node in fringe) {
                float nodeFScore = fScore[(int)node.x, (int)node.y]; 
                if (nodeFScore < minFScore) {
                    minFScore = nodeFScore;
                    current = node;
                }
            }

            //Debug.Log("AStar current is" + current);

            // Fond goal, return path in world coords
            if (current.Equals(goal)) { 
                List<Vector2> retracedPath = FindMyWayHome(cameFrom, current, start);

                //Debug.Log("Path in grid coords");
                for (int i = 0; i < retracedPath.Count; i++) {
                    //Debug.Log(retracedPath[i]);
                }
                //Debug.Log("Path in world coords");
                for (int i = 0; i < retracedPath.Count; i++) {
                    retracedPath[i] = GridToWorld(retracedPath[i]);
                    //Debug.Log(retracedPath[i]);
                }

                return retracedPath;

            }

            fringe.Remove(current);
            visited.Add(current);

            // Explore neighbors
            foreach (Vector2 neighbor in adjList[current]) {

                // Ignorne visited nodes
                if (visited.Contains(neighbor)) {
                    continue;
                }

                // Add unseen neighbors to fringe
                if (!fringe.Contains(neighbor)) {
                    fringe.Add(neighbor);
                }

                // We only care about this path if we have a better gScore
                float distToNeighbor = 1;
                float currentGScoreToNeighbor = gScore[(int)current.x, (int)current.y] + distToNeighbor; 
                if (currentGScoreToNeighbor >= gScore[(int)neighbor.x, (int)neighbor.y]) {
                    continue;
                }

                // This is the best path we have found yet. Write that down
                cameFrom[(int)neighbor.x, (int)neighbor.y] = current;
                gScore[(int)neighbor.x, (int)neighbor.y] = currentGScoreToNeighbor;
                fScore[(int)neighbor.x, (int)neighbor.y] = gScore[(int)neighbor.x, (int)neighbor.y] + ManhattanDistance(neighbor, goal);

            }
        }

        // We never found the goal :'(
        print("Failed to find AStar path");
        return null;
    }

    private List<Vector2> FindMyWayHome(Vector2[,] cameFrom, Vector2 current, Vector2 home) {
        List<Vector2> path = new List<Vector2>();
        path.Add(current);

        while (!current.Equals(home)) {
            current = cameFrom[(int)current.x, (int)current.y];
            path.Add(current);
        }

        return path;
    }

    private Dictionary<Vector2, HashSet<Vector2>> GetAdjacencyList() {
        Dictionary<Vector2, HashSet<Vector2>> result = new Dictionary<Vector2, HashSet<Vector2>>();
        HashSet<string> walkables = new HashSet<string>();

        int w = mapState.GetLength(0);
        int h = mapState.GetLength(1);
        for (int i = 1; i < w-1; i++) {
            for (int j = 1; j < h-1; j++) {
                if (IsWalkable(mapState[i, j])) {
                    // This is a floor tile
                    Vector2 tile = new Vector2(i, j);

                    // Add all it's floor neighbors to an adjacency list
                    List<Vector2> neighbors = new List<Vector2>();
                    neighbors.Add(new Vector2(i + 1, j));
                    neighbors.Add(new Vector2(i - 1, j));
                    neighbors.Add(new Vector2(i, j + 1));
                    neighbors.Add(new Vector2(i, j - 1));

                    neighbors.ForEach(neigh => {
                        if (IsWalkable(mapState[(int)neigh.x, (int)neigh.y])) {
                            if (!result.ContainsKey(tile)) {
                                result[tile] = new HashSet<Vector2>();
                            }
                            if (!result.ContainsKey(neigh)) {
                                result[neigh] = new HashSet<Vector2>();
                            }
                            result[tile].Add(neigh);
                            result[neigh].Add(tile);
                        }
                    });
                }
            }
        }

        /*
        print("These are things in 10 13");
        HashSet<Vector2> hs = result[new Vector2(10, 13)];
        foreach (Vector2 v2 in hs) {
            print(v2);
        }
        */

        return result;
    }

    private static bool IsWalkable(string s) {
        HashSet<string> nonWalkables = new HashSet<string>();
        nonWalkables.Add("Wall");
        return !(nonWalkables.Contains(s));
    }

    private static T[,] Populate2D<T>(T[,] arr, T value) {
        for (int i = 0; i < arr.GetLength(0); i++) {
            for (int j = 0; j < arr.GetLength(1); j++) {
                arr[i, j] = value;
            }
        }
        return arr;
    }

    private static float ManhattanDistance(Vector2 s, Vector2 e) {
        return Mathf.Abs(s.x - e.x) + Mathf.Abs(s.y - e.y);
    }

    private static void Print2DArray(string[,] arr) {
        string arrStr = "";
        for (int i = 0; i < arr.GetLength(0); i++) {
            for (int j = 0; j < arr.GetLength(1); j++) {
                arrStr += IsWalkable(arr[i, j]) ? "1" : "0";
                arrStr += ",";
            }
            arrStr += "\n";
        }
        print(arrStr);
    }
}
