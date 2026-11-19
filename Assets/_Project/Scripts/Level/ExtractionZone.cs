using System;
using Game.Player;
using UnityEngine;

namespace Game.Level
{
    public class ExtractionZone : MonoBehaviour
    {
        [SerializeField] private LevelZoneManager _zoneManager;

        public event Action OnLevelCompleted;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController _))
            {
                if (_zoneManager == null || _zoneManager.IsCleared)
                {
                    OnLevelCompleted?.Invoke();
                }
            }
        }
    }
}
