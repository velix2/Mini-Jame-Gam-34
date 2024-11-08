using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private bool DEBUG_DISABLE_START;
    
    public static GameHandler Instance;
    public bool IsWaveInProgress { get; private set; }
    public bool IsGameOver { get; private set; }
    public int CurrentWave => _currentWave + 1;
    
    [SerializeField] private int[] enemySpawnsPerWave;
    [SerializeField] private int maxEnemiesAlive = 10;
    [SerializeField] private int enemiesLeftForMarker = 3;
    [SerializeField] private float timeBetweenSpawnsInSecs = 5f;
    [SerializeField] private float timeBetweenWavesInSecs = 10f;

    [SerializeField] private int tomatoesToGameOver = 50;

    [SerializeField] private PlayerMovement player;
    [SerializeField] private ItemSpawner[] itemSpawners;
    
    
    [SerializeField] private GameObject plowPrefab;
    [SerializeField] private GameObject sowPrefab;
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject enemyMarkerPrefab;
    [SerializeField] private GameObject markerHolder;
    [SerializeField] private WaveText waveText;
    [SerializeField] private TomatoText tomatoText;
    [SerializeField] private GameObject tutorialHolder;

    [SerializeField] private GameObject normalCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private AudioClip gameOverSound;
    
    
    [System.Serializable]
    public class SpawnChances
    {
        public float plowChance = 0.45f;
        public float sowChance = 0.35f;
        public float waterChance = 0.2f;
    }
    
    [SerializeField] private SpawnChances spawnChances;
    [SerializeField] private int extraEnemiesPerWave = 2;
    
    
    private int _currentWave = -1;
    private readonly List<Enemy> _enemiesAlive = new ();
    private readonly HashSet<Item> _itemsAlive = new ();
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

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SubmitTomatoes(99);
        }
    }*/

    private void Start()
    {
        if(DEBUG_DISABLE_START) return;
        StartIntroPhase();
    }
    
    public void SubmitTomatoes(int tomatoes)
    {
        TomatoesCollected += tomatoes;
        tomatoText.UpdateText();

        //Game over
        if (TomatoesCollected < tomatoesToGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("Game over");
        GetComponent<AudioSource>().PlayOneShot(gameOverSound);
        normalCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
    }
        
    
    public int TomatoesCollected { get; private set; }

    public int TomatoesToGameOver => tomatoesToGameOver;

    private void StartIntroPhase()
    {
        StartCoroutine(IntroPhase());
        foreach (var spawner in itemSpawners)
        {
            spawner.OnPrepPhase();
        }
    }
    
    private IEnumerator IntroPhase()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        tutorialHolder.SetActive(false);
        
        foreach (var spawner in itemSpawners)
        {
            spawner.OnWaveStart();
        }
        StartWave();
    }
    
    private void StartWave()
    {
        _isEnemyMarkersActive = false;
        _currentWave++;
        if (_currentWave >= enemySpawnsPerWave.Length)
        {
            _enemiesToSpawn = enemySpawnsPerWave[^1] + extraEnemiesPerWave * (1 + _currentWave - enemySpawnsPerWave.Length);
        }
        else
        {
            _enemiesToSpawn = enemySpawnsPerWave[_currentWave];
        }
        IsWaveInProgress = true;
        waveText.UpdateText();
        StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        while (_enemiesToSpawn > 0)
        {
            int randomSpawnPointIndex = Random.Range(0, GameAreaHandler.Instance.EnemySpawnPoints.Length);
            Transform spawnPoint = GameAreaHandler.Instance.EnemySpawnPoints[randomSpawnPointIndex];
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
        Debug.Log("All enemies spawned");
        yield return new WaitUntil(() => EnemiesAliveCount == 0);
        CompleteWave();
    }

    private void CompleteWave()
    {
        Debug.Log("Wave complete");
        IsWaveInProgress = false;
        _isEnemyMarkersActive = false;
        waveText.UpdateText();
        ScoreHandler.Instance.Score += ScoreHandler.Instance.scoreValues.waveComplete;
        WipeAllItems();
        foreach (var spawner in itemSpawners)
        {
            spawner.OnPrepPhase();
        }
        StartCoroutine(PrepPhase());
    }
    
    public void ReportItemSpawn(Item item)
    {
        _itemsAlive.Add(item);
    }

    private void WipeAllItems()
    {
        player.TakeAwayItem();
        foreach (var item in _itemsAlive.Where(item => item))
        {
            Destroy(item.gameObject);
        }
    }
    
    private IEnumerator PrepPhase()
    {
        yield return new WaitForSeconds(timeBetweenWavesInSecs);
        foreach (var spawner in itemSpawners)
        {
            spawner.OnWaveStart();
        }
        StartWave();
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
        
        if (EnemiesAliveCount + _enemiesToSpawn == 0)
        {
            _isEnemyMarkersActive = false;
            Debug.Log("Markers inactive");
        }
        else if (EnemiesAliveCount + _enemiesToSpawn <= enemiesLeftForMarker && !_isEnemyMarkersActive)
        {
            _isEnemyMarkersActive = true;
            Debug.Log("Markers active");
            AssignMarkersToEnemies();
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
