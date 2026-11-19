using System;
using System.Collections.Generic;
using Game.Enemies;
using Game.Infrastructure;
using UnityEngine;
using Zenject;

namespace Game.Level
{
    public class LevelZoneManager : MonoBehaviour
    {
        [Serializable]
        private struct SpawnPointData
        {
            public EnemyType Type;
            public Transform Point;
            public Transform[] PatrolRoute;
        }

        [SerializeField] private List<SpawnPointData> _spawns;
        [SerializeField] private InteractiveDoor _arenaExitDoor;

        private readonly List<EnemyController> _aliveEnemies = new List<EnemyController>();
        private IEnemyFactory _enemyFactory;
        private bool _isCleared;

        public event Action<int> OnRemainingEnemiesChanged;
        public event Action OnAllZonesCleared;

        public bool IsCleared => _isCleared;

        [Inject]
        public void Construct(IEnemyFactory enemyFactory)
        {
            _enemyFactory = enemyFactory;
        }

        private void Start()
        {
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < _spawns.Count; i++)
            {
                SpawnPointData data = _spawns[i];
                if (data.Point == null)
                {
                    continue;
                }

                EnemyController enemy = _enemyFactory.Create(data.Type, data.Point.position, data.Point.rotation, data.PatrolRoute);
                enemy.OnEnemyDied += HandleEnemyDied;
                _aliveEnemies.Add(enemy);
            }

            OnRemainingEnemiesChanged?.Invoke(_aliveEnemies.Count);
        }

        private void HandleEnemyDied(EnemyController enemy)
        {
            enemy.OnEnemyDied -= HandleEnemyDied;
            _aliveEnemies.Remove(enemy);
            OnRemainingEnemiesChanged?.Invoke(_aliveEnemies.Count);

            if (_aliveEnemies.Count == 0 && !_isCleared)
            {
                _isCleared = true;
                if (_arenaExitDoor != null)
                {
                    _arenaExitDoor.OpenDoor();
                }

                OnAllZonesCleared?.Invoke();
            }
        }
    }
}
