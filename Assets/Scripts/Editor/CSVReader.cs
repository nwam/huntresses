using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class CSVReader {
    private const int NUM_COLS = 26;

    /**
     * Convert csv file to 2d array
     * Caveats: 
     * Don't put commas or newlines in the data
     * Will only read NUM_COLS cols left-to-right, any further cols are ignored.
     */
    public static bool[,] parseCSV(string csvPath) {
        string[] contents = File.ReadAllLines(csvPath);

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
}