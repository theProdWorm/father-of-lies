using UnityEngine;
using UnityEngine.Events;

namespace Helpers
{
    public class AwakeEventTrigger : MonoBehaviour
    {
        public UnityEvent AwakeCalled;
        public UnityEvent StartCalled;
        public UnityEvent OnEnableCalled;
        
        private void Awake()    => AwakeCalled?.Invoke();
        private void Start()    => StartCalled?.Invoke();
        private void OnEnable() => OnEnableCalled?.Invoke();
    }
}