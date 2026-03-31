using UnityEngine;
using UnityEngine.Events;

namespace Helpers.Events
{
    public class AnimationEventListener : MonoBehaviour
    {
        public UnityEvent OnAttackStarted;
        public UnityEvent OnAttackPerformed;
        public UnityEvent OnAttackEnded;
        public UnityEvent OnSwitch;
        public UnityEvent OnDeath;
        public UnityEvent OnIdle;
        public UnityEvent OnWolfJump;
        public UnityEvent OnWolfJumpEnd;
        public UnityEvent OnStepTaken;

        private void InvokeAttackStarted() => OnAttackStarted?.Invoke();
        private void InvokeAttackPerformed() => OnAttackPerformed?.Invoke();
        private void InvokeAttackEnded() => OnAttackEnded?.Invoke();
        private void InvokeSwitch() => OnSwitch?.Invoke();
        private void InvokeDeath() => OnDeath?.Invoke();
        private void InvokeIdle() => OnIdle?.Invoke();
        private void InvokeOnWolfJump() => OnWolfJump?.Invoke();
        private void InvokeOnWolfJumpEnd() => OnWolfJumpEnd?.Invoke();
        private void InvokeStepTaken() => OnStepTaken?.Invoke();
    }
}
