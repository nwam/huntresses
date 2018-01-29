using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMap : MonoBehaviour {

    private bool[,] levelMap;

	void Start () {
        CSVReader csvReader = new CSVReader();
        string levelName = SceneManager.GetActiveScene().name;
        levelMap = csvReader.ParseCSV(levelName + ".csv");
	}
}
