using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using UnityEngine;
using Zenject;

namespace Netologia.Systems
{
	public class ProjectileSystem : GameObjectPoolContainer<Projectile>, Director.IManualUpdate
	{
		private EffectSystem _effects;		//injected
		
		[SerializeField, Min(0.01f)]
		private float _hitDistance = 0.3f;
		
		public void ManualUpdate()
		{
			float deltaTime = TimeManager.DeltaTime;

			foreach (var pool in this)
			{
				foreach (var projectile in pool)
				{
					// 2.1. Смещаем снаряд в сторону цели
					Vector3 currentPos = projectile.transform.position;
					Vector3 targetPos = projectile.TargetPosition;
					Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, projectile.MoveSpeed * deltaTime);
					projectile.transform.position = nextPos;

					// 2.2. Если цель в зоне поражения
					if (Vector3.SqrMagnitude(targetPos - nextPos) <= _hitDistance)
					{
						// 2.2.1. Создаём хит-эффект
						if (projectile.HasEffect)
						{
							var effect = _effects[projectile.HitEffect].Get;
							effect.transform.position = nextPos;
							effect.Play();
						}

						// 2.2.2. Отыгрываем звук попадания
						if (projectile.HasSound)
						{
							AudioManager.PlayHit(projectile.HitSound);
						}

						// 2.2.3. Убираем снаряд обратно в пул
						this[projectile.Ref].ReturnElement(projectile.ID);

						// 2.2.4. Наносим урон и накладываем элементальный эффект цели
						projectile.DealDamage();
					}
				}
			}
		}
		

		public void OnDespawnUnit(int unitID)
		{
			foreach (var pool in this)
				foreach (var projectile in pool)
					if(projectile.TargetID == unitID)
						projectile.ResetTarget();
		}

		[Inject]
		private void Construct(EffectSystem effects)
		{
			(_effects) = (effects);
			//SqrtMagnitude optimization
			_hitDistance *= _hitDistance;
		}
	}
}