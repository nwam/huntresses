// To be implemented by GameObjects which interact with the TimeBubble
public interface IFreezable {
    bool isDestroyed();

    void Freeze();
    void UnFreeze();
}
