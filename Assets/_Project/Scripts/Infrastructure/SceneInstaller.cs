using Zenject;

namespace Game.Infrastructure
{
    public class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPoolService>().To<PoolService>().AsSingle();
        }
    }
}
