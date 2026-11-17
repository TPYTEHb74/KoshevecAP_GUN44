using Game.Player;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        public override void InstallBindings()
        {
            Container.Bind<IPoolService>().To<PoolService>().AsSingle();

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
