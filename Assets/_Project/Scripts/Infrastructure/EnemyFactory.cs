using Game.Enemies;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure
{
    public interface IEnemyFactory
    {
        EnemyController Create(EnemyType type, Vector3 position, Quaternion rotation, Transform[] patrolPoints = null);
    }

    public class EnemyFactory : IEnemyFactory
    {
        private readonly DiContainer _container;
        private readonly EnemyDatabase _database;

        public EnemyFactory(DiContainer container, EnemyDatabase database)
        {
            _container = container;
            _database = database;
        }

        public EnemyController Create(EnemyType type, Vector3 position, Quaternion rotation, Transform[] patrolPoints = null)
        {
            EnemyController prefab = _database.GetPrefab(type);
            EnemyController enemy = _container.InstantiatePrefabForComponent<EnemyController>(prefab, position, rotation, null);
            enemy.SetPatrolPoints(patrolPoints);
            return enemy;
        }
    }
}
