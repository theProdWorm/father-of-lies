using Abilities;
using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public abstract class PlayableCharacter : MonoBehaviour
    {
        private static readonly int ATTACK = Animator.StringToHash("attack");
        private static readonly int SWITCH = Animator.StringToHash("switch");
        private static readonly int DASH = Animator.StringToHash("dash");
        private static readonly int IS_MOVING = Animator.StringToHash("isMoving");
        private static readonly int SPEED = Animator.StringToHash("speed");

        [SerializeField] protected Ability _attackAbility;
        [SerializeField] protected Ability _switchAbility;

        [SerializeField] protected GameObject _model;
        [SerializeField] protected Animator _animator;

        [SerializeField] protected Transform _attackPoint;
        
        [Header("Target Lock")]
        [SerializeField] private float _targetLockAngle;

        [SerializeField] private float _targetLockMaxDistance;
        [SerializeField] private float _targetLockAngleWeight;
        [SerializeField] private float _targetLockDistanceWeight;

        [SerializeField] private bool _useCameraDirection = true;
        
        protected PlayerEntity _playerEntity;
        
        protected AbilityTracker _attackAbilityTracker;
        protected AbilityTracker _switchAbilityTracker;

        protected Entity _target;
        
        public bool IsSwitchReady => _switchAbilityTracker.RemainingCooldownPercent <= 0;
        public float SwitchCooldownPercent => _switchAbilityTracker.RemainingCooldownPercent;
        
        protected virtual void Start()
        {
            _playerEntity = PlayerEntity.INSTANCE;
            
            _attackAbilityTracker = new(_attackAbility, PrepareAttack);
            _switchAbilityTracker = new(_switchAbility, PrepareSwitch);
        }

        protected virtual void OnEnable()
        {
            _model.SetActive(true);
            
            if (!PlayerController.USE_SWITCH_ABILITY)
                return;
            
            _animator.SetTrigger(SWITCH);
        }
        
        protected virtual void OnDisable()
        {
            _model.SetActive(false);
        }

        protected virtual void Update()
        {
            _attackAbilityTracker.Update();
            _switchAbilityTracker.Update();
            
            _animator.SetBool(IS_MOVING, PlayerMovement.IS_MOVING);
        }
        
        public void TriggerDashAnimation() => _animator.SetTrigger(DASH);
        public void SetAnimationSpeed(float speed) => _animator.SetFloat(SPEED, speed);
        
        protected virtual void PrepareAttack()
        {
            _animator.SetTrigger(ATTACK);
            
            PlayerController.LoseControl();
            
            _target = FindTarget();
        }

        protected virtual void PerformAttack()
        {
            PlayerController.GainControl();
            FMODEvents.INSTANCE.PlayEvent(FMODEvents.INSTANCE._playerAttack, transform.position);
        }
        
        protected virtual void PrepareSwitch()
        {
        }

        protected virtual void PerformSwitch()
        {
            
        }

        protected abstract Entity FindTarget();
    }
}