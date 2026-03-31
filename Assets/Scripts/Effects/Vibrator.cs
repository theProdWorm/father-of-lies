using System.Collections;
using UnityEngine;

namespace Animation
{
    public class Vibrator : MonoBehaviour
    {
        [SerializeField] private float _intensity;
        [SerializeField] private float _duration;
        
        private Vector3 _offset;
        
        public void Vibrate()
        {
            StartCoroutine(VibrateCoroutine());
        }

        private IEnumerator VibrateCoroutine()
        {
            float elapsedTime = 0;
            while (elapsedTime < _duration)
            {
                transform.position -= _offset;
                _offset = Random.insideUnitSphere * _intensity;
                
                transform.position += _offset;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position -= _offset;
        }
    }
}