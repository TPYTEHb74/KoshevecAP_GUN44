using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using Zenject;
using UnityEngine;

namespace Netologia.Systems
{
	public class TowerSystem : GameObjectPoolContainer<Tower>, Director.IManualUpdate
	{
		private UnitSystem _units;				//injected
		private ProjectileSystem _projectiles;	//injected

		public void ManualUpdate()
		{
			float deltaTime = TimeManager.DeltaTime;

			foreach (var pair in this)
			{
				foreach (var tower in pair)
				{
					//1.1 Усли бышня в перезарядке
					if (!tower.DecrementAttackReload(deltaTime))
						continue;

					//1.2 Проверка есть ли у башни цель и не вышла ли она из видимости
					if(tower.HasTarget)
					{
						float sqrDistance = Vector3.SqrMagnitude(tower.Target.transform.position - tower.transform.position);
						if (sqrDistance > tower.Range *tower.Range || tower.Target.CurrentHealth <= 0f)
							tower.Target = null;
					}

                    // 1.2.1 - 1.2.3. Если цели нет — ищем ближайшую в радиусе атаки
					if (!tower.HasTarget)
					{
						Unit nearestTarget = _units.FindTarget(tower.transform.position, tower.Range);
						if (nearestTarget == null)
							continue;

						tower.Target = nearestTarget;
					}

                    // 1.3. Стреляем: создаём снаряд из пула и запускаем перезарядку
                    Projectile projectile = _projectiles[tower.Projectile].Get;
                    projectile.PrepareData(tower.transform.position, tower.Target, tower.Damage, tower.AttackElemental);
                    tower.Attack();


                }
            }
		}

		public void OnDespawnUnit(int unitID)
		{
			foreach (var pair in this)
				foreach (var tower in pair)
					if (tower.TargetID == unitID)
						tower.Target = null;
		}
		
		[Inject]
		private void Construct(UnitSystem units, ProjectileSystem projectiles)
		{
			_units = units;
			_projectiles = projectiles;
		}
	}
}