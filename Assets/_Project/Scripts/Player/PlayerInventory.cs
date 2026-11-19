using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        private readonly HashSet<string> _keys = new HashSet<string>();

        public void AddKey(string keyId)
        {
            if (!string.IsNullOrEmpty(keyId))
            {
                _keys.Add(keyId);
            }
        }

        public bool HasKey(string keyId)
        {
            return string.IsNullOrEmpty(keyId) || _keys.Contains(keyId);
        }
    }
}
