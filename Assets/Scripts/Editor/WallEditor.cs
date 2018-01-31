using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

public class WallEditor : EditorWindow {

    private string[] scenes;
    private string[] levelCsvs;

    private int selectedSceneIndex = 0;
    private int selectedLevelIndex = 0;

    private int btnWidth = 100;
    private int btnHeight = 25;

    private CSVReader csvReader;

    [MenuItem("GameObject/Daddy's Amazing Wall Generator")]
    static void OpenWindow() {
        GetWindow(typeof(WallEditor), false, title: "Generate Walls with Daddy");
    }

    private void OnEnable() {
        csvReader = new CSVReader();
        scenes = loadScenes();
        levelCsvs = csvReader.GetFilenames();
    }

    private void OnGUI() {
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scene Name:");
        GUILayout.FlexibleSpace();
        selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, scenes);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Level CSV Name:");
        GUILayout.FlexibleSpace();
        selectedLevelIndex = EditorGUILayout.Popup(selectedLevelIndex, levelCsvs);
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Close", GUILayout.Width(btnWidth), GUILayout.Height(btnHeight))) {
            Close();
        }
        GUILayout.FlexibleSpace();

        if(GUILayout.Button("Refresh", GUILayout.Width(btnWidth), GUILayout.Height(btnHeight))) {
            scenes = loadScenes();
            levelCsvs = csvReader.GetFilenames();
        }

        GUILayout.FlexibleSpace();
        if(GUILayout.Button("GENERATE", GUILayout.Width(btnWidth), GUILayout.Height(btnHeight))) {
            // Do generation
            Scene scene = getScene(scenes[selectedSceneIndex]);
            CSVReader csvReader = new CSVReader();
            bool[,] walls = csvReader.ParseCSV(levelCsvs[selectedLevelIndex]);
            addWallsToScene(scene, walls);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private string[] loadScenes() {
        string[] result = new string[SceneManager.sceneCount];
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            result[i] = SceneManager.GetSceneAt(i).name;
        }
        return result;
    }


    private Scene getScene(string sceneName) {
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName) {
                return scene;
            }
        }
        Debug.LogError("Unknown scene " + sceneName);
        return new Scene();     // ?????
    }

    private void addWallsToScene(Scene scene, bool[,] locations) {
        //EditorSceneManager.OpenScene(scene.path);

        GameObject floorFab = GameObject.Find("Floor");
        GameObject wallFab = GameObject.Find("Wall");
        if(wallFab == null || floorFab == null) {
            Debug.LogError("You must have a GameObject in the scene called \"Wall\" and one called \"Floor\"");
            return;
        }

        int width = locations.GetLength(0);
        int height = locations.GetLength(1);

        // First, create a plane with the same dimensions as locations array
        GameObject floor = Instantiate(floorFab, new Vector3(0, 0, 1), Quaternion.identity);
        floor.transform.localScale = new Vector2(width, height);

        GameObject wallParent = new GameObject("Wall Daddy");
        int wallCount = 0;
        for (int i = 0; i < locations.GetLength(0); i++) {
            for (int j = 0; j < locations.GetLength(1); j++) {
                if(locations[j, i]) {
                    //Debug.Log("A wall at " + j + ", " + i);
                    wallCount++;
                    Instantiate(wallFab, Pathfinding.GridToWorld(new Vector2(i,j), height, width), Quaternion.identity, wallParent.transform);
                }
            }
        }
        Debug.Log("Created " + wallCount + " walls.");

        //Undo.RecordObject(floorFab, "Undo Floor Generation");
        //Undo.RecordObject(wallParent, "Undo Wall Generation");
        EditorSceneManager.MarkSceneDirty(scene);
        //EditorSceneManager.SaveScene(scene);
    }
}