// To be implemented by things which can harvest corpses.
public interface IHarvester
{
    void AddHarvestTarget(Corpse corpse);
    void RemoveHarvestTarget(Corpse corpse);
}
