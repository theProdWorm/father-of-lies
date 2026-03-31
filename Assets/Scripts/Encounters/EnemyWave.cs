using System.Collections.Generic;
using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    [SerializeField, Tooltip("This is purely for organisation purposes, does not affect the code what so ever")]
    private string _name;
    public List<EncounterManager.EnemyTypes> Enemies = new List<EncounterManager.EnemyTypes>();
}
