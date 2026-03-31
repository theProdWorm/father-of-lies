using Entities;
using UnityEngine;
using UnityEngine.Events;

namespace StatusEffects
{
    public abstract class StatusEffect
    {
        public bool Expired;

        public bool Stackable;
        public bool RefreshOnApplication;
        
        private float _maxDuration;
        private float _remainingDuration;

        public StatusEffect(float duration, bool stackable, bool refreshOnApplication)
        {
            _maxDuration = duration;
            _remainingDuration = duration;
            
            Stackable = stackable;
            RefreshOnApplication = refreshOnApplication;
        }
        
        public abstract void Apply(Enemy enemy);
        public abstract void Remove(Enemy enemy);

        public void RefreshDuration()
        {
            _remainingDuration = _maxDuration;
        }

        public void Update()
        {
            _remainingDuration -= Time.deltaTime;
            
            if (_remainingDuration <= 0)
                Expired = true;
        }
    }
}