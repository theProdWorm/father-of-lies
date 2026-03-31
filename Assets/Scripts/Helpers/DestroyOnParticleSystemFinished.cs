using UnityEngine;

namespace Helpers
{
    public class DestroyOnParticleSystemFinished : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private float _elapsedTime;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime < _particleSystem.main.startLifetime.constantMax)
                return;
            
            bool finished = _particleSystem.particleCount == 0;
            
            if (finished)
                Destroy(gameObject);
        }
    }
}
