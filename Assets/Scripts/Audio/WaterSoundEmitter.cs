using UnityEngine;

namespace Audio
{
    public class WaterSoundEmitter : MonoBehaviour
    {
        [SerializeField] private float _maxDistance;
        
        private static Transform _player;
        
        private void Update()
        {
            var distance = _player.position - transform.position;
            distance.y = 0;
            
            var volume = Mathf.Clamp01(distance.magnitude / _maxDistance);
            
            var distanceX = distance.x;
            var distanceZ = distance.z;
            
            
        }
    }
}