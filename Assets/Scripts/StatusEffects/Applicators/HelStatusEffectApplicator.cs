using Entities;
using StatusEffects.Effects;
using UnityEngine;

namespace StatusEffects.Applicators
{
    public class HelStatusEffectApplicator : StatusEffectApplicator
    {
        [SerializeField] private int _maxChill;
        [SerializeField] private float _slowAmount = 0.05f;
        
        [SerializeField] private float _frozenDuration;
        
        protected override void ApplyEffect(Enemy enemy, int _)
        {
            if (enemy.ImmuneToStatusEffects || !enemy.HasSpawned || enemy.HasStatusEffectOfType<StatusEffect_Frozen>())
                return;
            
            for (int i = 0; i < _stacksToApply; i++)
            {
                var chillEffect = new StatusEffect_Chill(_duration, _stackable, _refresh, _slowAmount);
                enemy.ApplyStatusEffect(chillEffect);
            
                int count = enemy.CountStatusEffectsOfType<StatusEffect_Chill>();
                if (count < _maxChill)
                    continue;
                
                enemy.RemoveAllStatusEffectsOfType<StatusEffect_Chill>();
                
                var frozenEffect = new StatusEffect_Frozen(_frozenDuration);
                enemy.ApplyStatusEffect(frozenEffect);
                
                break;
            }
        }
    }
}