# TPS Shooter — Vertical Slice (Unity 2022.3.62f3 URP)

Вертикальный срез шутера от третьего лица с архитектурой на **Extenject (Zenject)**, асинхронностью на **UniTask** и анимациями на **DOTween**.

## Технические характеристики
* **Версия Unity:** `2022.3.62f3` (Universal Render Pipeline)
* **Ветка репозитория:** `tps_shooter`
* **Сцены в Build Settings:** `0: MainMenu`, `1: Gameplay`

## Управление
* **WASD / Стрелки** — перемещение
* **Мышь** — обзор
* **Shift** — спринт
* **C** — приседание
* **Space** — прыжок
* **ЛКМ** — стрельба
* **ПКМ** — прицеливание (смена камеры Cinemachine и FOV)
* **R** — перезарядка (асинхронная через UniTask)
* **1–5 / Колёсико мыши** — переключение оружия (Пистолет, Автомат, Дробовик, Гранатомёт, Рельсотрон)
* **V** — удар в ближнем бою (с повышенным уроном в спину)
* **E** — взаимодействие с объектами (двери, терминалы)
* **Tab** — открыть/закрыть большую карту уровня
* **Esc** — пауза

## Использованные паттерны проектирования

| Паттерн | Реализация в проекте | Назначение |
| :--- | :--- | :--- |
| **Dependency Injection** | `ProjectInstaller`, `SceneInstaller` (Zenject) | Внедрение зависимостей без синглтонов и `FindObjectOfType` |
| **Factory Method** | `IEnemyFactory`, `EnemyFactory` | Кастомная фабрика создания врагов разных типов через контейнер |
| **State Machine** | `IEnemyState`, `EnemyPatrolState`, `EnemyChaseState`, `EnemyAttackState`, `EnemyDeadState` | Управление поведением ИИ врагов (Штурмовик и Стрелок) |
| **Strategy** | `IShootStrategy`, `HitscanShootStrategy`, `ProjectileShootStrategy`, `PiercingBeamShootStrategy` | Изоляция алгоритмов баллистики разных видов оружия |
| **Object Pool** | `IPoolService`, `PoolService` | Переиспользование трассеров пуль и снарядов гранатомёта без лишних аллокаций |
| **Observer** | C# `event Action` в `HealthComponent`, `WeaponController`, `InputService`, `LevelZoneManager` | Реактивное обновление HUD без опроса в `Update` |

## Оптимизация и проверка памяти
* Использованы `NonAlloc` методы физики (`Physics.RaycastNonAlloc`, `Physics.OverlapSphereNonAlloc`).
* Восприятие ИИ (`Field of View` + `Linecast`) выполняется по таймеру (`0.15 сек`) вместо каждого кадра.
* Все задачи `UniTask` привязаны к `CancellationTokenSource` с `GetCancellationTokenOnDestroy()`.
* Все анимации `DOTween` привязаны через `.SetLink(gameObject)`.

![UniTask Tracker](unitask_tracker.png)
