using System.Collections;
using UnityEngine;

namespace Effects
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] private Transform _openPosition;
        [SerializeField] private Transform _closedPosition;
        [SerializeField] private float _duration = 2f;
        [SerializeField] private float _shakeIntensity = 0.5f;
        [SerializeField] private ParticleSystem _particleSystem;
        
        private bool _isOpen;
        
        public void Toggle()
        {
            StartCoroutine(_isOpen ? OpenRoutine() : CloseRoutine());

            _isOpen = !_isOpen;
        }
        
        public void Open() => StartCoroutine(OpenRoutine());
        public void Close() => StartCoroutine(CloseRoutine());

        private IEnumerator OpenRoutine()
        {
            _particleSystem.Play();
            
            float elapsedTime = 0;
            while (elapsedTime < _duration)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeIntensity;
                
                float t = Mathf.Clamp01(elapsedTime / _duration);
                transform.position = Vector3.Lerp(_closedPosition.position, _openPosition.position, t) + shakeOffset;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator CloseRoutine()
        {
            _particleSystem.Play();
            
            float elapsedTime = 0;
            while (elapsedTime < _duration)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeIntensity;
                
                float t = Mathf.Clamp01(elapsedTime / _duration);
                transform.position = Vector3.Lerp(_openPosition.position, _closedPosition.position, t) + shakeOffset;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}