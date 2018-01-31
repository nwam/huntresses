using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

public class CSVReader {
    private const int NUM_COLS = 26;
    private string folder = Application.streamingAssetsPath + "/levelMaps";

    /**
     * Convert csv file to 2d array
     * Caveats: 
     * Don't put commas or newlines in the data
     * Will only read NUM_COLS cols left-to-right, any further cols are ignored.
     */
    public bool[,] ParseCSV(string filename) {
        string[] contents = File.ReadAllLines(folder + "/" + filename);

        bool[,] result = new bool[contents.Length, NUM_COLS];

        for (int i = 0; i < contents.Length; i++) {
            string[] row = contents[i].Split(',');
            for (int j = 0; j < NUM_COLS; j++) {
                //Debug.Log(j + ", " + i + " = " + row[j]);
                // A w means a wall in this location
                result[j, i] = row[j] == "w";
            }
        }

        return result;
    }

    public string[] GetFilenames() {
        return Directory.GetFiles(folder, "*.csv")
            .Select(lvl => Path.GetFileName(lvl))
            .ToArray();
    }
}