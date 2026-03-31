using Entities;

namespace StatusEffects.Effects
{
    public class StatusEffect_Chill : StatusEffect
    {
        private readonly float _slowAmount;
        
        public StatusEffect_Chill(float duration, bool stackable, bool refreshOnApplication, float slowAmount) : 
            base(duration, stackable, refreshOnApplication)
        {
            _slowAmount = slowAmount;
        }
        
        public override void Apply(Enemy enemy)
        {
            enemy.RemoveSpeedMultiplier(_slowAmount);
        }

        public override void Remove(Enemy enemy)
        {
            enemy.AddSpeedMultiplier(_slowAmount);
        }
    }
}