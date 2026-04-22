using Entities;
using System.Collections;
using System.Collections.Generic;
using Entities.Player;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private static PlayerEntity PLAYER;

    [SerializeField]
    private List<Transform> _reSpawnPoints = new();

    private static int RESPAWN_INDEX;
    public static bool DIED;

    private void Start()
    {
        PLAYER = PlayerEntity.INSTANCE;
        
        if (DIED)
        {
            DIED = false;
            
            var playerPos = PLAYER.transform.position;
            var spawnPoint = _reSpawnPoints[RESPAWN_INDEX];
            var spawnPos = spawnPoint.position;
            spawnPos.y = playerPos.y;
            
            PLAYER.transform.position = spawnPos;
            PLAYER.transform.rotation = spawnPoint.rotation;
        }
        else
            StartCoroutine(WalkThroughDoorRoutine());
    }

    private IEnumerator WalkThroughDoorRoutine()
    {
        var playerPos = PLAYER.transform.position;
        var spawnPoint = _reSpawnPoints[0];
        var spawnPos = spawnPoint.position;
        spawnPos.y = playerPos.y;

        PLAYER.transform.position = spawnPos;
        PLAYER.transform.rotation = spawnPoint.rotation;

        PlayerMovement.SetDashing(true);
        yield return new WaitForSecondsRealtime(1);
        PlayerMovement.SetDashing(false);
    }

    public static void Proceed() => RESPAWN_INDEX++;
    
    public static void ResetProgress() => RESPAWN_INDEX = 0;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var item in _reSpawnPoints)
        {
            Vector3 pos = item.position;

            Gizmos.DrawWireSphere(pos, .5f);

            Gizmos.DrawRay(item.position, item.transform.forward * 2);
        }
    }
}
