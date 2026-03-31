using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability")]
    public class Ability : ScriptableObject
    {
        public GameObject AttackPrefab;
        
        [Header("Charges")]
        public int  MaxCharges = 1;
        [Tooltip("Time between charges in seconds.")]
        public float RechargeTime;
        [Tooltip("If enabled, regains all charges at once when cooldown ends.")]
        public bool SimultaneousRecharge;
        [Tooltip("Disallows using the ability unless max amount of charges are available.")]
        public bool  RequireMaxCharges;

        [Header("Burst")]
        [Tooltip("Whether to use all charges at once.")]
        public bool  Burst;
        [Tooltip("Time between charge uses in a burst.")]
        public float BurstDelay;

        [Tooltip("Angle in degrees between each attack in a spread.")]
        public float SpreadAngle;
    }
}