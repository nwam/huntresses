// To be implemented by GameObjects which interact with the TimeBubble
public interface IFreezable {
    bool IsDestroyed();

    void Freeze();
    void UnFreeze();
}
