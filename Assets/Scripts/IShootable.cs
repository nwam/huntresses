// Interface to be implemented by things which interact meaningfully with bullets.
// Implement the GetShot method to respond to getting hit by a bullet.
public interface IShootable {
    void GetShot(int damage);
}
