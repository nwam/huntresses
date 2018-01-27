// To be implemented by GameObjects which interact with the TimeBubble
using UnityEngine;

public interface IFreezable {
    bool isDestroyed();

    void Freeze();
    void UnFreeze();
}
