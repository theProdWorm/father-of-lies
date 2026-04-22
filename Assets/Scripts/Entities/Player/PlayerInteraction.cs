using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float _lookWeight;
        [SerializeField] private float _distanceWeight;
        
        private readonly List<IInteractable> _interactables = new();
        private IInteractable _currentInteractable;

        private void Update()
        {
            if (_interactables.Count > 0)
                FindMainInteractable();
        }

        private void FindMainInteractable()
        {
            if (_currentInteractable != null)
                _currentInteractable.Highlighted = false;

            int lowestIndex = 0;
            float highestScore = 0;
            for (int i = 0; i < _interactables.Count; i++)
            {
                var between = (_interactables[i].Position - transform.position);
                var distance = between.magnitude;
                var direction = between / distance;

                float distScore = 1 - Mathf.Clamp01(distance / 10f);
                var dot = Vector3.Dot(transform.forward, direction);

                float score = dot * _lookWeight + distScore * _distanceWeight;
                if (score > highestScore)
                {
                    lowestIndex = i;
                    highestScore = score;
                }
            }

            _currentInteractable = _interactables[lowestIndex];
            _currentInteractable.Highlighted = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                _interactables.Add(other.GetComponent<IInteractable>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                IInteractable interactable = other.GetComponent<IInteractable>();

                if (interactable == _currentInteractable)
                    _currentInteractable.Highlighted = false;

                _interactables.Remove(interactable);
            }
        }

        public void InteractInput(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_interactables.Count == 0 || _currentInteractable == null)
                return;

            _currentInteractable.Interacted();
        }
    }
}