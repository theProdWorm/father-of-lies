using UnityEngine;
using Random = UnityEngine.Random;

namespace Animation
{
    public class Firework : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;

        [SerializeField] private float   _force;
        [SerializeField] private float   _horizontality;
        
        [SerializeField] private bool    _forceDirection;
        [SerializeField] private Vector2 _direction;
        
        [SerializeField] private GameObject _explosionPrefab;

        private bool _isLit;

        public void LightFuse()
        {
            if (_isLit)
                return;

            _rigidbody.isKinematic = false;
            
            _isLit = true;

            Vector2 direction = _horizontality * (_forceDirection ? _direction.normalized : Random.insideUnitCircle);
            Vector3 explosionPosition = new Vector3(direction.x, -1, direction.y).normalized;
            
            explosionPosition += transform.position;

            _rigidbody.AddExplosionForce(_force, explosionPosition, 10f, 1, ForceMode.Impulse);
        }

        private void Explode()
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_isLit)Explode();
        }

        
    }
}