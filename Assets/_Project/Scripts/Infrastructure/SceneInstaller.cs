using Game.Enemies;
using Game.Player;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        public override void InstallBindings()
        {
            Container.Bind<IPoolService>().To<PoolService>().AsSingle();
            Container.BindInstance(_enemyDatabase).AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            Container.Bind<PlayerController>()
                .FromComponentInNewPrefab(_playerPrefab)
                .UnderTransformGroup("Player")
                .AsSingle()
                .OnInstantiated<PlayerController>((context, player) =>
                {
                    player.transform.SetPositionAndRotation(_playerSpawnPoint.position, _playerSpawnPoint.rotation);
                })
                .NonLazy();
        }
    }
}
