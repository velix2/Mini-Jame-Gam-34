using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;
    
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private int[] enemySpawnsPerWave;
    [SerializeField] private int maxEnemiesAlive = 10;
    [SerializeField] private float timeBetweenSpawnsInSecs = 5f;
    [SerializeField] private float prepTimeInSecs = 3f;
    
    [SerializeField] private GameObject enemyPrefab;
    
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
        _currentWave++;
        _enemiesToSpawn = enemySpawnsPerWave[_currentWave];
        StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(prepTimeInSecs);
        while (_enemiesToSpawn > 0)
        {
            int randomSpawnPointIndex = Random.Range(0, enemySpawnPoints.Length);
            Transform spawnPoint = enemySpawnPoints[randomSpawnPointIndex];
            Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity).GetComponent<Enemy>();
            _enemiesAlive.Add(enemy);
            _enemiesToSpawn--;
            yield return new WaitUntil(() => EnemiesAliveCount < maxEnemiesAlive);
            float randomTimeBetweenSpawns = Random.Range(timeBetweenSpawnsInSecs * 0.75f, timeBetweenSpawnsInSecs * 1.25f);
            yield return new WaitForSeconds(randomTimeBetweenSpawns);
        }
    }
    
    public void EnemyRemoved(Enemy enemy)
    {
        _enemiesAlive.Remove(enemy);
    }

}
