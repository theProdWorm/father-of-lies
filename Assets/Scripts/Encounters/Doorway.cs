using UnityEngine;

public class Doorway : MonoBehaviour
{
    [SerializeField] 
    private EncounterManager _encounterHandler;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _encounterHandler.StartEncounter();
    }
}
