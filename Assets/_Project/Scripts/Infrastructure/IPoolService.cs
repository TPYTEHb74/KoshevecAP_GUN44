using UnityEngine;

namespace Game.Infrastructure
{
    public interface IPoolService
    {
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;
        void Despawn<T>(T instance) where T : Component;
    }
}
