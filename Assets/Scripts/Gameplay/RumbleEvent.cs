using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "New Rumble Event", menuName = "Gameplay/Rumble Event")]
    public class RumbleEvent : ScriptableObject
    {
        public ParticleSystem.MinMaxCurve LowFrequency;
        public ParticleSystem.MinMaxCurve HighFrequency;
        
        public float Duration;
    }
}