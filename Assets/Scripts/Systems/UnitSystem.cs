using Behaviours;
using JetBrains.Annotations;
using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using Netologia.TowerDefence.Settings;
using System;
using UnityEngine;
using Zenject;

namespace Netologia.Systems
{
    public class UnitSystem : GameObjectPoolContainer<Unit>, Director.IManualUpdate
    {
        private Director _director;                 //injected
        private EffectSystem _effects;              //injected
        private Constants _constants;               //injected
        private Vector3[] _path;                    //injected

        [SerializeField, Min(0.01f)]
        private float _arrivalDistance = 0.1f;

        public event Action<int> OnDespawnUnitHandler;

        [CanBeNull]
        public Unit FindTarget(in Vector3 position, float range)
        {
            range *= range;
            var target = default(Unit);
            foreach (var pair in this)
            {
                foreach (var unit in pair)
                {
                    var distance = Vector3.SqrMagnitude(unit.transform.position - position);
                    if (distance < range)
                    {
                        (range, target) = (distance, unit);
                    }
                }
            }

            return target;
        }

        public void ManualUpdate()
        {
            float deltaTime = TimeManager.DeltaTime;
            float currentTime = TimeManager.Time;

            foreach (var pool in this)
            {
                foreach (var unit in pool)
                {
                    // Обновление дебаффов огня и льда + периодический урон от горения
                    int fireStacks = unit.CountEffect(ElementalType.Fire);
                    if (fireStacks > 0)
                    {
                        float burnDamage = unit.Stats.Health * _constants.FireDebuffDamageMult * fireStacks * deltaTime;
                        unit.CurrentHealth -= burnDamage;
                        unit.TryRemoveEffect(currentTime, ElementalType.Fire);
                    }

                    if (unit.CountEffect(ElementalType.Ice) > 0)
                    {
                        unit.TryRemoveEffect(currentTime, ElementalType.Ice);
                    }

                    // 3.1 - 3.2. Если ХП закончилось — убираем врага, начисляем золото и отыгрываем эффекты
                    if (unit.CurrentHealth <= 0f)
                    {
                        DespawnUnit(unit, unit.transform.position);
                        continue;
                    }

                    // 3.3. Если ХП есть — двигаемся к следующей точке маршрута
                    if (_path == null || _path.Length == 0)
                    {
                        continue;
                    }

                    Vector3 targetWaypoint = _path[unit.PathIndex];
                    Vector3 nextPosition = Vector3.MoveTowards(unit.transform.position, targetWaypoint, unit.MoveSpeed * deltaTime);
                    unit.transform.position = nextPosition;
                    unit.Visual.ManualUpdate(deltaTime);


                    // Проверяем достижение текущей точки маршрута
                    if (Vector3.SqrMagnitude(targetWaypoint - nextPosition) <= _arrivalDistance)
                    {
                        unit.PathIndex++;

                        // 3.4. Если достигнута последняя точка (выход) — убираем врага и наносим урон игроку
                        if (unit.PathIndex >= _path.Length)
                        {
                            _director.AddPlayerDamage(1);
                            this[unit.Ref].ReturnElement(unit.ID);
                            OnDespawnUnitHandler?.Invoke(unit.ID);
                        }
                    }
                }
            }
        }

        private void DespawnUnit(Unit unit, in Vector3 position)
        {
            //Create hitEffect
            if (unit.HasEffect)
            {
                var effect = _effects[unit.DieEffect].Get;
                effect.transform.position = position;
                effect.Play();
            }

            //Play sound
            if (unit.HasSound)
            {
                AudioManager.PlayHit(unit.DieSound);
            }

            _director.AddMoney(unit.Stats.Cost);
            this[unit.Ref].ReturnElement(unit.ID);
            OnDespawnUnitHandler?.Invoke(unit.ID);
        }

        [Inject]
        private void Construct(EffectSystem effects, Director director, Constants constants, WaveController path)
        {
            (_effects, _director, _constants, _path) = (effects, director, constants, path.GetPath());
            _arrivalDistance *= _arrivalDistance;
            AwakeMethod = t => t.Constants = _constants;
        }
    }
}
