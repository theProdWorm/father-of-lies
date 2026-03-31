using System.Collections;
using UnityEngine;

namespace Effects
{
    public class Rustler : MonoBehaviour
    {
        [SerializeField] private GameObject _idleObject;
        [SerializeField] private GameObject _rustledObject;
        
        [SerializeField] private ParticleSystem _particleSystem;
        
        public void Rustle(float duration)
        {
            StartCoroutine(RustleRoutine(duration));
        }

        private IEnumerator RustleRoutine(float duration)
        {
            _idleObject.SetActive(false);
            _rustledObject.SetActive(true);
            
            _particleSystem.Stop();
            _particleSystem.Clear();
            _particleSystem.Play();
            
            yield return new WaitForSeconds(duration);
            
            _idleObject.SetActive(true);
            _rustledObject.SetActive(false);
        }
    }
}