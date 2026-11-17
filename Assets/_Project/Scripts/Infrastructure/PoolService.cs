using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure
{
    public class PoolService : IPoolService
    {
        private readonly DiContainer _container;
        private readonly Dictionary<int, Queue<Component>> _pools = new Dictionary<int, Queue<Component>>();
        private readonly Dictionary<int, int> _instanceToPrefabId = new Dictionary<int, int>();
        private readonly Transform _poolRoot;

        public PoolService(DiContainer container)
        {
            _container = container;
            _poolRoot = new GameObject("[ObjectPool]").transform;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            int prefabId = prefab.GetInstanceID();

            if (!_pools.TryGetValue(prefabId, out Queue<Component> pool))
            {
                pool = new Queue<Component>();
                _pools.Add(prefabId, pool);
            }

            T instance = null;
            while (pool.Count > 0 && instance == null)
            {
                instance = pool.Dequeue() as T;
            }

            if (instance == null)
            {
                instance = _container.InstantiatePrefabForComponent<T>(prefab, position, rotation, parent);
                _instanceToPrefabId[instance.gameObject.GetInstanceID()] = prefabId;
            }
            else
            {
                Transform targetTransform = instance.transform;
                targetTransform.SetParent(parent);
                targetTransform.SetPositionAndRotation(position, rotation);
                instance.gameObject.SetActive(true);
            }

            if (instance is IPoolable poolable)
            {
                poolable.OnSpawned();
            }

            return instance;
        }

        public void Despawn<T>(T instance) where T : Component
        {
            if (instance == null)
            {
                return;
            }

            if (instance is IPoolable poolable)
            {
                poolable.OnDespawned();
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_poolRoot);

            int instanceId = instance.gameObject.GetInstanceID();
            if (_instanceToPrefabId.TryGetValue(instanceId, out int prefabId) && _pools.TryGetValue(prefabId, out Queue<Component> pool))
            {
                pool.Enqueue(instance);
            }
            else
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
