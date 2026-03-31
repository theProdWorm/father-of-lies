using Entities;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Events;

public class EncounterManager : MonoBehaviour
{
    public UnityEvent _encounterStart;
    public UnityEvent _encounterEnd;

    private static Player _player;
    public enum EnemyTypes
    {
        Draugr,
        BirdOnBird,
        Wolf,
        WolfBoss
    }

    [SerializeField] List<GameObject> _gates = new();

    [Header("Enemy Spawn Points")]
    [SerializeField, Tooltip("Represented in percentage form"), Range(0, 100)]
    private int _chanceForDraugrToRunThroughDoor = 25;
    [SerializeField, Tooltip("Set to -1 to turn off")]
    private float minimumDistanceFromPlayerToSpawnDraugr = -1f;
    [SerializeField, Tooltip("Set to -1 to turn off")]
    private float maximumDistanceFromPlayerToSpawnDraugr = -1f;
    [SerializeField]
    private List<GameObject> _draugrSpawnPoints = new();

    [SerializeField]
    private List<GameObject> _birdOnBirdSpawnPoints = new();

    [SerializeField]
    private List<GameObject> _wolfSpawnPoints = new(); //TODO: Maybe just have wolves run in through the doors instead of having spawn points for them?

    [Header("Enemy Waves"), SerializeField, Tooltip("These are the enemies that are supposed to already be in the room")]
    private EnemyWave _wave0;
    //[SerializeField]
    //private bool _spawnRandomLocationsForWave0 = false;
    [SerializeField, Tooltip("Just add the EnemyWave script as a component below to edit it, then drag and drop that component to add it to the list")]
    private List<EnemyWave> _enemyWaves = new();

    [SerializeField, Tooltip("The minimum time in seconds between each wave of enemies, set to -1 if you want to turn it off")]
    private float _timeBetweenWaves = -1f;
    private float _timeSinceLastWave = 0f;

    [SerializeField, Tooltip("Represented in decimal form"), Range(0, 1)]
    private float _percentageOfEnemiesToSpawnNextWave = 25;

    [Header("Time Between Spawns Per Wave")]
    [SerializeField]
    private float _minSpawnTime = .25f;
    [SerializeField]
    private float _maxSpawnTime = .75f;

    private List<Entity> _enemiesAlive = new();
    private float _currentAmountOfEnemiesAlive = 0;
    [Tooltip("Adds together the amount of enemies that were left over from last wave and the ones that spawn in the new wave")]
    private int _amountOfEnemiesThisWave = 0;
    private int _currentWaveIndex = 0;

    private bool _isEncounterActive = false;
    private bool _isEncounterCompleted = false;
    private bool _isSpawning = false;

    private float _timeBetweenChecks = .1f;
    private float _t = 0f;

    private void OnDisable()
    {
        foreach (Entity enemy in _enemiesAlive)
        {
            enemy.OnDeath.RemoveListener(EnemyDied);
        }
    }

    public void StartEncounter()
    {
        if (_isEncounterActive || _isEncounterCompleted)
            return;
        _isEncounterActive = true;
        _player = FindFirstObjectByType<Player>();
        CloseDoors();

        if (_wave0 != null)
            ActivateFirstWave();
        else
            NextWave();
    }

    private void Update()
    {
        if (!_isEncounterActive || _isEncounterCompleted || _isSpawning) return;

        _t += Time.deltaTime;

        if (_t >= _timeBetweenChecks)
        {

            //Time between waves matters
            if (_timeBetweenWaves != -1)
            {
                _timeSinceLastWave += _t;
                if (_timeSinceLastWave >= _timeBetweenWaves)
                {
                    NextWave();
                }
            }

            float _percentageOfEnemiesLeft = _currentAmountOfEnemiesAlive / _amountOfEnemiesThisWave;
            if (_percentageOfEnemiesLeft <= _percentageOfEnemiesToSpawnNextWave && _isSpawning == false)
            {
                NextWave();
            }
            _t = 0;
        }
    }

    private void EnemyDied(Entity enemy)
    {
        //TODO: Add a father enemy class and put that here instead of the test enemy
        if (enemy is Enemy)
        {
            _enemiesAlive.Remove(enemy);
            _currentAmountOfEnemiesAlive = _enemiesAlive.Count;
            Debug.Log("Enemy died, " + _currentAmountOfEnemiesAlive + " enemies left alive");
        }
    }

    private void NextWave()
    {
        _timeSinceLastWave = 0;
        if (_currentWaveIndex >= _enemyWaves.Count - 1)
        {
            if (_enemiesAlive.Count <= 0)
            {
                _isEncounterCompleted = true;
                OpenDoors();
            }
            return;
        }
        _isSpawning = true;

        List<EnemyTypes> nextWaveEnemies = _enemyWaves[_currentWaveIndex].Enemies;

        _amountOfEnemiesThisWave = nextWaveEnemies.Count + (int)_currentAmountOfEnemiesAlive;

        StartCoroutine(SpawnWave(nextWaveEnemies));
        _currentAmountOfEnemiesAlive = _enemiesAlive.Count;

        _currentWaveIndex++;
    }

    private void ActivateFirstWave()
    {
        List<EnemyTypes> nextWaveEnemies = _wave0.Enemies;
        _amountOfEnemiesThisWave = nextWaveEnemies.Count;
        _currentAmountOfEnemiesAlive = _amountOfEnemiesThisWave;
        StartCoroutine(SpawnWave(nextWaveEnemies));
    }

    private System.Collections.IEnumerator SpawnWave(List<EnemyTypes> wave)
    {
        //Randomizes the wave list to keep player on their toes
        List<EnemyTypes> tempList = new();
        List<int> closedList = new();

        for (int i = 0; i < wave.Count;)
        {
            int r = Random.Range(0, wave.Count);

            if (!closedList.Contains(r))
            {
                tempList.Add(wave[i]);
                i++;
                closedList.Add(r);
            }
        }
        wave = tempList;

        //Spawn each enemy in randomized list
        for (int i = 0; i < wave.Count; i++)
        {
            Entity entity = null;
            switch (wave[i])
            {
                case EnemyTypes.Draugr:
                    entity = SpawnEnemy(_draugrSpawnPoints, true);
                    break;
                case EnemyTypes.BirdOnBird:
                    entity = SpawnEnemy(_birdOnBirdSpawnPoints, false);
                    break;
                case EnemyTypes.Wolf:
                    entity = SpawnEnemy(_wolfSpawnPoints, false);
                    break;
            }

            if (entity != null)
            {
                entity.OnDeath.AddListener(EnemyDied);
                _enemiesAlive.Add(entity);
            }
            else
            {
                Debug.Log("entity returned null");
            }

            float r = Random.Range(_minSpawnTime, _maxSpawnTime);
            yield return new WaitForSecondsRealtime(r);
        }
        _isSpawning = false;
    }

    private void CloseDoors()
    {
        _encounterStart.Invoke();
        foreach (GameObject gate in _gates)
        {
            gate.GetComponent<Gateway>().Close();
        }
        
        FMODEvents.INSTANCE.SetCombat(true);
    }

    private void OpenDoors()
    {
        _encounterEnd.Invoke();
        foreach (GameObject gate in _gates)
        {
            gate.GetComponent<Gateway>().Open();
        }
        FMODEvents.INSTANCE.SetCombat(false);
    }

    private Entity SpawnEnemy(List<GameObject> spawnPoints, bool removePoint)
    {
        Entity entity = null;
        int r = Random.Range(1, 101);
        #region Distance Checks
        List<GameObject> _tooCloseSpawnPoints = new();
        List<GameObject> _tooFarSpawnPoints = new();

        foreach (GameObject statue in spawnPoints)
        {
            float distance = Vector2.Distance(statue.transform.position, _player.transform.position);
            if (distance < minimumDistanceFromPlayerToSpawnDraugr && minimumDistanceFromPlayerToSpawnDraugr != -1)
                _tooCloseSpawnPoints.Add(statue);
            if (distance > maximumDistanceFromPlayerToSpawnDraugr && maximumDistanceFromPlayerToSpawnDraugr != -1)
                _tooFarSpawnPoints.Add(statue);
        }
        #endregion

        #region Ideal Spawn Distance

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            r = Random.Range(0, spawnPoints.Count);

            //Check distance availability
            if (_tooCloseSpawnPoints.Contains(spawnPoints[r]) || _tooFarSpawnPoints.Contains(spawnPoints[r]))
                continue;

            SpawnPoint spawnPointScript = spawnPoints[r].GetComponent<SpawnPoint>();
            if (removePoint)
                spawnPoints.RemoveAt(r);
            return spawnPointScript.Spawn();
        }

        #endregion

        #region Search Far

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            r = Random.Range(0, spawnPoints.Count);

            //Check distance availability
            if (_tooCloseSpawnPoints.Contains(spawnPoints[r]))
                continue;

            SpawnPoint spawnPointScript = spawnPoints[r].GetComponent<SpawnPoint>();
            if (removePoint)
                spawnPoints.RemoveAt(r);
            return spawnPointScript.Spawn();
        }

        #endregion

        #region Search everywhere

        if (spawnPoints.Count == 0)
            return null;

        r = Random.Range(0, spawnPoints.Count);
        Debug.Log(r + ", " + spawnPoints.Count);
        entity = spawnPoints[r].GetComponent<SpawnPoint>().Spawn();
        if (removePoint)
            spawnPoints.RemoveAt(r);
        return entity;

        #endregion
    }
}