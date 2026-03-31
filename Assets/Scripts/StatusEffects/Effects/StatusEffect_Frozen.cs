using Entities;

namespace StatusEffects.Effects
{
    public class StatusEffect_Frozen : StatusEffect
    {
        public StatusEffect_Frozen(float duration) : 
            base(duration, false, false)
        {
        }
        
        public override void Apply(Enemy enemy)
        {
            enemy.Freeze();
        }

        public override void Remove(Enemy enemy)
        {
            enemy.Unfreeze();
        }
    }
}