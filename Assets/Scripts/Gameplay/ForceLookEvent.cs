using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "New Force Look Event", menuName = "Gameplay/Force Look Event")]
    public class ForceLookEvent : ScriptableObject
    {
        [Tooltip("How long to hold at the target before going back")]
        [SerializeField] public float ForcedLookAtHoldTime = 2f;
        [Tooltip("How long it takes to lerp to the target")]
        [SerializeField] public float ForcedLookAtLerpTime = 1f;
        [SerializeField] public ParticleSystem.MinMaxCurve ForcedLookAtCurve;
        
        [Tooltip("How long it takes to lerp back to the original target")]
        [SerializeField] public float LookBackLerpTime = 1f;
        [SerializeField] public ParticleSystem.MinMaxCurve LookBackCurve;
    }
}