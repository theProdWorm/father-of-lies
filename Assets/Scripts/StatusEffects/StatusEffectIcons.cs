using UnityEngine;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "Status Effect Icons", menuName = "Status Effects/Status Effect Icons")]
    public class StatusEffectIcons : ScriptableObject
    {
        [Tooltip("The total time for an icon to pulse when a new stack is applied.")]
        [SerializeField] public float ChillPulseDuration;
        [SerializeField] public float ChillPulseFrequency;
        [SerializeField] public float ChillPulseIntensity;
        
        [SerializeField] public float FrozenPulseDuration;
        [SerializeField] public float FrozenPulseFrequency;
        [SerializeField] public float FrozenPulseIntensity;
        
        [SerializeField] public Sprite ChillIcon;
        [SerializeField] public Sprite FrozenIcon;
        
        [SerializeField] public Color MinStackColor;
        [SerializeField] public Color MaxStackColor;

        [SerializeField] public Sprite[] NumberSprites;
    }
}