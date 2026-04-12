using System.Collections.Generic;
using UnityEngine;

public class ProgressPersistence : MonoBehaviour
{
    public static bool FirstBranchDone;
    public static bool SecondBranchDone;

    private static readonly Dictionary<EncounterManager, bool> _encounterStates = new();
    
    private void Start()
    {
        SceneManager.OnSceneExit.AddListener(Start);
        
        var encounters = FindObjectsByType<EncounterManager>(FindObjectsSortMode.None);
        foreach (var encounter in encounters)
        {
            if (!_encounterStates.TryAdd(encounter, false) && _encounterStates[encounter])
            {
                encounter.DisableEncounter();
            }
        }
    }
}
