using System;

namespace Abilities
{
    public class AttackAbilityTracker : AbilityTracker
    {
        private readonly Action<Ability, int> _onAbilityUsed;
        
        public AttackAbilityTracker(Ability ability, Action<Ability, int> onAbilityUsed) 
            : base(ability)
        {
            _onAbilityUsed = onAbilityUsed;
        }
        
        public override bool TryUse()
        {
            if (_remainingCharges == 0)
                return false;

            if (_ability.RequireMaxCharges && _remainingCharges != _ability.MaxCharges)
                return false;

            int useTimes = _ability.Burst ? _ability.MaxCharges : 1;
            _remainingCharges -= useTimes;
            
            if (_ability.SimultaneousRecharge || _remainingCooldown <= 0)
                _remainingCooldown = _ability.RechargeTime;
            
            _onAbilityUsed?.Invoke(_ability, useTimes);

            return true;
        }
    }
}