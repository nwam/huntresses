// To be implemented by GameObjects which are considered in path finding
public interface IPathLogic {
    float Priority();
    string MapKey();
}