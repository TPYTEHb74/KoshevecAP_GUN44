using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Configs/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        [Serializable]
        private struct EnemyEntry
        {
            public EnemyType Type;
            public EnemyController Prefab;
        }

        [SerializeField] private List<EnemyEntry> _enemies;

        public EnemyController GetPrefab(EnemyType type)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Type == type)
                {
                    return _enemies[i].Prefab;
                }
            }

            throw new KeyNotFoundException($"Префаб для врага типа {type} не найден в EnemyDatabase!");
        }
    }
}
