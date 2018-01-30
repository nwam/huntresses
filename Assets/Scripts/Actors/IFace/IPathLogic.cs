using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// To be implemented by GameObjects which are considered in path finding
public interface IPathLogic {
    void onSpawn();
    float Priority();
    string MapKey();
}