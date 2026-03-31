using UnityEngine;
using UnityEngine.Events;

namespace Helpers
{
    public class LifetimeLimiter : MonoBehaviour
    {
        [SerializeField] private float _lifetime;
        
        [SerializeField] public UnityEvent OnDestroy;

        private float _timeLived;
        
        private void Update()
        {
            _timeLived += Time.deltaTime;
            
            if (_timeLived >= _lifetime)
            {
                OnDestroy?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}