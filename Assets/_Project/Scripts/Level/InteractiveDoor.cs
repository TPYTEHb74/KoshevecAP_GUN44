using DG.Tweening;
using Game.Player;
using UnityEngine;

namespace Game.Level
{
    public class InteractiveDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _doorLeaf;
        [SerializeField] private Vector3 _openLocalOffset = new Vector3(0f, 3.2f, 0f);
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private string _requiredKeyId;

        private Vector3 _closedLocalPos;
        private bool _isOpen;

        public bool CanInteract => !_isOpen;
        public string InteractionPrompt => string.IsNullOrEmpty(_requiredKeyId)
            ? "Нажмите [E], чтобы открыть дверь"
            : $"Требуется ключ: {_requiredKeyId} [E]";

        private void Awake()
        {
            if (_doorLeaf != null)
            {
                _closedLocalPos = _doorLeaf.localPosition;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_isOpen || _doorLeaf == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_requiredKeyId))
            {
                if (!interactor.TryGetComponent(out PlayerInventory inventory) || !inventory.HasKey(_requiredKeyId))
                {
                    _doorLeaf.DOShakePosition(0.3f, 0.1f).SetLink(gameObject);
                    return;
                }
            }

            OpenDoor();
        }

        public void OpenDoor()
        {
            if (_isOpen || _doorLeaf == null)
            {
                return;
            }

            _isOpen = true;
            _doorLeaf.DOLocalMove(_closedLocalPos + _openLocalOffset, _duration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);
        }
    }
}
