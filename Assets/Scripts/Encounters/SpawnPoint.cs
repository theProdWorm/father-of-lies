using Effects;
using UnityEngine;
using Entities;
using Helpers;
using UnityEngine.AI;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField, Tooltip("The sort of enemy that spawns here")]
    private EncounterManager.EnemyTypes _enemyType;

    [Header("Draugr")]
    [SerializeField]
    private bool _sittingStatue = false;
    public bool hasSpawned = false;

    Animator animator;

    private float _animatorSpeed; //Holder for the animation speed of current clip

    [Header("UGLY SOLUTION, WILL BE FIXED")]
    [SerializeField]
    private Enemy _enemyScript;
    [SerializeField]
    private Behaviour _behaviourAgentComponent;
    [SerializeField]
    private NavMeshAgent _navMeshAgentComponent;

    [Header("Bird on Bird")]
    [SerializeField]
    private GameObject _bbPrefab;

    [Header("Wolf")]
    [SerializeField]
    private GameObject _wolfPrefab;

    [Header("BossWolf")]
    [SerializeField]
    private GameObject _bossWolfPrefab;

    private void Start()
    {
        if (!hasSpawned && _enemyType is EncounterManager.EnemyTypes.Draugr)
            PrepareDraugrStatue();
    }

    public Entity Spawn()
    {
        switch (_enemyType)
        {
            case EncounterManager.EnemyTypes.Draugr:
                return SpawnDraugr();
            case EncounterManager.EnemyTypes.BirdOnBird:
                return SpawnBirdOnBird();
            case EncounterManager.EnemyTypes.Wolf:
                return SpawnWolf();
            case EncounterManager.EnemyTypes.WolfBoss:
                return SpawnBossWolf();
            default:
                break;
        }

        return null;
    }

    private void PrepareDraugrStatue()
    {
        animator = GetComponent<Animator>();

        animator.SetBool("HasSpawned", false);
        animator.SetBool("Sitting", _sittingStatue);

        #region Ugly solution
        _enemyScript.enabled = false;
        _behaviourAgentComponent.enabled = false;
        _navMeshAgentComponent.enabled = false;
        #endregion

        _animatorSpeed = animator.speed;
        animator.speed = 0f;

        Enemy enemyScript = GetComponent<Enemy>();
        enemyScript.HasSpawned = false;

        //TODO: Pick a random frame in the animation
    }

    private Entity SpawnDraugr()
    {
        //Safety check
        if (hasSpawned) return null;
        hasSpawned = true;

        Entity _entity = GetComponent<Entity>();

        #region Ugly solution
        _enemyScript.enabled = true;
        _behaviourAgentComponent.enabled = true;
        _navMeshAgentComponent.enabled = true;
        #endregion

        animator.SetBool("HasSpawned", true);

        animator.speed = _animatorSpeed;

        Enemy enemyScript = GetComponent<Enemy>();
        enemyScript.HasSpawned = true;

        return _entity;
    }

    private Entity SpawnBirdOnBird()
    {
        GameObject bird = Instantiate(_bbPrefab, transform.position, transform.rotation);
        Entity entity = bird.GetComponent<Entity>();
        return entity;
        //TODO: OBJECTPOOL
    }

    private Entity SpawnWolf()
    {
        GameObject wolf = Instantiate(_wolfPrefab, transform.position, transform.rotation);
        Entity entity = wolf.GetComponent<Entity>();
        
        GetComponent<Rustler>().Rustle(1f);
        
        return entity;
        //TODO: OBJECTPOOL
    }

    private Entity SpawnBossWolf()
    {
        GameObject boss = Instantiate(_bossWolfPrefab, transform.position, transform.rotation);
        Entity entity = boss.GetComponent<Entity>();
        return entity;
        //TODO: OBJECTPOOL
    }
}