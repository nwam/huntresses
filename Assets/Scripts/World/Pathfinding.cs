using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pathfinding : MonoBehaviour {

    private bool[,] wallMap;
    Dictionary<Vector2, HashSet<Vector2>> adjList;

	void Start () {
        CSVReader csvReader = new CSVReader();
        string levelName = SceneManager.GetActiveScene().name;
        wallMap = csvReader.ParseCSV(levelName + ".csv");
        adjList = GetAdjacencyList();
	}

    // Start and goal are return list are in world coordinates
    public List<Vector2> AStar(Vector2 start, Vector2 goal) {
        start = WorldToGrid(start, wallMap.GetLength(0), wallMap.GetLength(1));
        goal = WorldToGrid(goal, wallMap.GetLength(0), wallMap.GetLength(1));
        print(start);
        print(goal);
        HashSet<Vector2> visited = new HashSet<Vector2>();
        List<Vector2> fringe = new List<Vector2>();

        Vector2[,] cameFrom = new Vector2[wallMap.GetLength(0), wallMap.GetLength(1)];
        
        // Cost of getting from start to node
        float[,] gScore = new float[wallMap.GetLength(0), wallMap.GetLength(1)];
        gScore = Populate2D<float>(gScore, Mathf.Infinity);

        // Estimated cost to goal
        float[,] fScore = new float[wallMap.GetLength(0), wallMap.GetLength(1)];
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

            // Fond goal, return path in world coords
            if (current.Equals(goal)) {
                List<Vector2> retracedPath = RetracePath(cameFrom, current);
                for (int i = 0; i < retracedPath.Count; i++) {
                    retracedPath[i] = GridToWorld(retracedPath[i], wallMap.GetLength(0), wallMap.GetLength(1));
                }
            }

            fringe.RemoveAt(0);
            visited.Add(current);

            // Explore neighbors
            print(current);
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
        return null;
    }

    private List<Vector2> RetracePath(Vector2[,] cameFrom, Vector2 current) {
        List<Vector2> path = new List<Vector2>();
        path.Add(current);

        while (cameFrom[(int)current.x, (int)current.y] != null) {
            current = cameFrom[(int)current.x, (int)current.y];
            path.Add(current);
        }

        return path;
    }

    private Dictionary<Vector2, HashSet<Vector2>> GetAdjacencyList() {
        Dictionary<Vector2, HashSet<Vector2>> result = new Dictionary<Vector2, HashSet<Vector2>>();

        int w = wallMap.GetLength(0);
        int h = wallMap.GetLength(1);
        for (int i = 1; i < w-1; i++) {
            for (int j = 1; j < h-1; j++) {
                if (!wallMap[i, j]) {
                    // This is a floor tile
                    Vector2 tile = new Vector2(i, j);

                    // Add all it's floor neighbors to an adjacency list
                    List<Vector2> neighbors = new List<Vector2>();
                    neighbors.Add(new Vector2(i + 1, j));
                    neighbors.Add(new Vector2(i - 1, j));
                    neighbors.Add(new Vector2(i, j + 1));
                    neighbors.Add(new Vector2(i, j - 1));

                    neighbors.ForEach(neigh => {
                        if (!wallMap[(int)neigh.x, (int)neigh.y]) {
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

        return result;
    }

    public static Vector2 GridToWorld(Vector2 gridPoint, int height, int width) {
        return new Vector2(gridPoint.x - width / 2 + 0.5f, -1 * (gridPoint.y - height / 2 + 0.5f));
    }

    public static Vector2 WorldToGrid(Vector2 worldPoint, int height, int width) {
        worldPoint = RoundWorld(worldPoint);
        return new Vector2(worldPoint.x + width/2 - 0.5f, (-1*worldPoint.y + height/2 - 0.5f));
        
    }

    private static Vector2 RoundWorld(Vector2 worldPoint) {
        return new Vector2((int)worldPoint.x + 0.5f, (int)worldPoint.y + 0.5f);
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

}
