using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;
    public bool IsWaveInProgress { get; private set; }
    
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private int[] enemySpawnsPerWave;
    [SerializeField] private int maxEnemiesAlive = 10;
    [SerializeField] private int enemiesLeftForMarker = 3;
    [SerializeField] private float timeBetweenSpawnsInSecs = 5f;
    [SerializeField] private float prepTimeInSecs = 3f;
    [SerializeField] private float timeBetweenWavesInSecs = 10f;

    
    [SerializeField] private GameObject plowPrefab;
    [SerializeField] private GameObject sowPrefab;
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject enemyMarkerPrefab;
    [SerializeField] private GameObject markerHolder;
    
    
    [System.Serializable]
    public class SpawnChances
    {
        public float plowChance = 0.45f;
        public float sowChance = 0.35f;
        public float waterChance = 0.2f;
    }
    
    [SerializeField] private SpawnChances spawnChances;
    
    private int _currentWave = -1;
    private readonly List<Enemy> _enemiesAlive = new ();
    private int _enemiesToSpawn;
    
    private int EnemiesAliveCount => _enemiesAlive.Count;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        SetUpPool();
    }
    
    private void Start()
    {
        StartPrepPhase();
    }
    
    private void StartPrepPhase()
    {
        StartCoroutine(PrepPhase());
    }
    
    private IEnumerator PrepPhase()
    {
        //TODO. Explain game
        yield return new WaitForSeconds(prepTimeInSecs);
        StartWave();
    }
    
    private void StartWave()
    {
        _isEnemyMarkersActive = false;
        _currentWave++;
        _enemiesToSpawn = enemySpawnsPerWave[_currentWave];
        IsWaveInProgress = true;
        StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(prepTimeInSecs);
        while (_enemiesToSpawn > 0)
        {
            int randomSpawnPointIndex = Random.Range(0, enemySpawnPoints.Length);
            Transform spawnPoint = enemySpawnPoints[randomSpawnPointIndex];
            Enemy enemy = Instantiate(GetRandomEnemyPrefab(), spawnPoint.position, Quaternion.identity).GetComponent<Enemy>();
            _enemiesAlive.Add(enemy);
            _enemiesToSpawn--;
            
            if (_isEnemyMarkersActive)
            {
                enemy.AssignMarker(GetMarkerFromPool());
            }
            
            yield return new WaitUntil(() => EnemiesAliveCount < maxEnemiesAlive);
            float randomTimeBetweenSpawns = Random.Range(timeBetweenSpawnsInSecs * 0.75f, timeBetweenSpawnsInSecs * 1.25f);
            yield return new WaitForSeconds(randomTimeBetweenSpawns);
        }
        //Wave complete
        Debug.Log("Wave complete");
        IsWaveInProgress = false;
        ScoreHandler.Instance.Score += ScoreHandler.Instance.scoreValues.waveComplete;
        yield return new WaitForSeconds(timeBetweenWavesInSecs);
    }
    
    private GameObject GetRandomEnemyPrefab()
    {
        var i = Random.value;
        if (i < spawnChances.plowChance)
        {
            return plowPrefab;
        }
        if (i < spawnChances.plowChance + spawnChances.sowChance)
        {
            return sowPrefab;
        }
        return waterPrefab;
    }
    
    public void EnemyRemoved(Enemy enemy)
    {
        _enemiesAlive.Remove(enemy);
        
        if (EnemiesAliveCount + _enemiesToSpawn <= enemiesLeftForMarker)
        {
            _isEnemyMarkersActive = true;
            Debug.Log("Markers active");
            AssignMarkersToEnemies();
        } else if (EnemiesAliveCount + _enemiesToSpawn == 0)
        {
            _isEnemyMarkersActive = false;
            Debug.Log("Markers inactive");
        }

    }

    #region EnemyMarkers

    private readonly List<MarkerScript> _enemyMarkersPool = new ();
    private bool _isEnemyMarkersActive;

    private void SetUpPool()
    {

        for (int i = 0; i < enemiesLeftForMarker; i++)
        {
            var marker = Instantiate(enemyMarkerPrefab, markerHolder.transform);
            marker.SetActive(false);
            _enemyMarkersPool.Add(marker.GetComponent<MarkerScript>());
        }
    }
    
    private void AssignMarkersToEnemies()
    {
        foreach (var enemy in _enemiesAlive)
        {
            if (enemy.HasMarker) continue;
            enemy.AssignMarker(GetMarkerFromPool());
        }
    }
    
    private MarkerScript GetMarkerFromPool()
    {
        if (_enemyMarkersPool.Count == 0)
        {
            var marker = Instantiate(enemyMarkerPrefab, markerHolder.transform).GetComponent<MarkerScript>();
            _enemyMarkersPool.Add(marker);
        }
        var enemyMarker = _enemyMarkersPool[0];
        _enemyMarkersPool.RemoveAt(0);
        return enemyMarker;
    }

    public void ReturnMarkerToPool(MarkerScript marker)
    {
        marker.gameObject.SetActive(false);
        _enemyMarkersPool.Add(marker);
    }

    #endregion

}
