using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abilities
{
    public class AbilityTracker
    {
        private readonly Action _onAbilityUsed;
        
        protected readonly Ability _ability;
        
        protected float _remainingCooldown;
        protected int   _remainingCharges;

        public float RemainingCooldownPercent => _remainingCooldown / _ability.RechargeTime;
        
        public AbilityTracker(Ability ability, Action onAbilityUsed)
        {
            _ability = ability;
            
            _onAbilityUsed = onAbilityUsed;
            
            _remainingCharges = ability.MaxCharges;
        }

        protected AbilityTracker(Ability ability)
        {
            _ability = ability;
            
            _remainingCharges = ability.MaxCharges;
        }

        public void Reset()
        {
            _remainingCooldown = _ability.RechargeTime;
            _remainingCharges  = 0;
        }

        public void Update()
        {
            if (_remainingCharges == _ability.MaxCharges)
                return;
            
            _remainingCooldown -= Time.deltaTime;
            if (_remainingCooldown > 0)
                return;
            
            if (_ability.SimultaneousRecharge)
            {
                _remainingCharges = _ability.MaxCharges;
            }
            else
            {
                _remainingCharges++;

                if (_remainingCharges != _ability.MaxCharges)
                    _remainingCooldown = _ability.RechargeTime;
            }
        }

        public virtual bool TryUse()
        {
            if (_remainingCharges == 0)
                return false;
            
            if (_ability.RequireMaxCharges && _remainingCharges != _ability.MaxCharges)
                return false;

            int useTimes = _ability.Burst ? _ability.MaxCharges : 1;
            _remainingCharges -= useTimes;
            
            if (_ability.SimultaneousRecharge || _remainingCooldown <= 0)
                _remainingCooldown = _ability.RechargeTime;
            
            _onAbilityUsed?.Invoke();

            return true;
        }
    }
}