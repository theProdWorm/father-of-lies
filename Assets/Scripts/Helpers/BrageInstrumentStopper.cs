using UnityEngine;

namespace Helpers
{
    public class BrageInstrumentStopper : MonoBehaviour
    {
        private static readonly int STOP_MUSIC = Animator.StringToHash("stopMusic");
        
        private Animator _animator;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void StopMusic()
        {
            _animator.SetTrigger(STOP_MUSIC);
        }
    }
}