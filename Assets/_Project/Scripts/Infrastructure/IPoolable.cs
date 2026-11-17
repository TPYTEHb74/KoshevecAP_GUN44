namespace Game.Infrastructure
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
