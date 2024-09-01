using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameAreaHandler : MonoBehaviour
{
    [SerializeField] private Vector3Int topLeftCorner;
    [SerializeField] private Vector3Int bottomRightCorner;
    [SerializeField] private float entryPointMaxRandomOffset;
    [SerializeField] private Transform enemySpawnPointsHolder;
    [SerializeField] private Transform enemyEntryPointsHolder;

    private Transform[] _enemySpawnPoints;
    private Transform[] _enemyEntryPoints;

    private Vector3 _topLeftCornerWorldPosition;
    private Vector3 _bottomRightCornerWorldPosition;

    public static GameAreaHandler Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _enemySpawnPoints = new Transform[enemySpawnPointsHolder.childCount];
        for (int i = 0; i < enemySpawnPointsHolder.childCount; i++)
        {
            _enemySpawnPoints[i] = enemySpawnPointsHolder.GetChild(i);
        }
        
        _enemyEntryPoints = new Transform[enemyEntryPointsHolder.childCount];
        for (int i = 0; i < enemyEntryPointsHolder.childCount; i++)
        {
            _enemyEntryPoints[i] = enemyEntryPointsHolder.GetChild(i);
        }
    }
    
    public Transform[] EnemySpawnPoints => _enemySpawnPoints;

    private void Start()
    {
        _topLeftCornerWorldPosition = FieldHandler.Instance.CellToWorldCentered(topLeftCorner);
        _bottomRightCornerWorldPosition = FieldHandler.Instance.CellToWorldCentered(bottomRightCorner);
    }
    
    public bool IsPositionInsideGameArea(Vector3 position)
    {
        return position.x >= _topLeftCornerWorldPosition.x && position.x <= _bottomRightCornerWorldPosition.x &&
               position.y >= _bottomRightCornerWorldPosition.y && position.y <= _topLeftCornerWorldPosition.y;
    }
    
    public Vector3 GetRandomWorldPositionInsideGameArea()
    {
        float randomX = UnityEngine.Random.Range(_topLeftCornerWorldPosition.x, _bottomRightCornerWorldPosition.x + 1);
        float randomY = UnityEngine.Random.Range(_bottomRightCornerWorldPosition.y, _topLeftCornerWorldPosition.y + 1);
        return new Vector3(randomX, randomY, 0);
    }
    
    public Vector3Int GetRandomCellPositionInsideGameArea()
    {
        int randomX = UnityEngine.Random.Range(topLeftCorner.x, bottomRightCorner.x + 1);
        int randomY = UnityEngine.Random.Range(bottomRightCorner.y, topLeftCorner.y + 1);
        return new Vector3Int(randomX, randomY, 0);
    }
    
    public Vector3 GetClosestEntryPointPosition(Vector3 position)
    {
        return GetClosestEntryPointTransform(position).position + (Vector3) Random.insideUnitCircle * entryPointMaxRandomOffset;
    }
    
    private Transform GetClosestEntryPointTransform(Vector3 position)
    {
        return _enemyEntryPoints.OrderBy(x => Vector3.Distance(x.position, position)).First();
    }
    
    public Vector3 GetClosestSpawnPointPosition(Vector3 position)
    {
        return GetClosestSpawnPointTransform(position).position;
    }
    
    private Transform GetClosestSpawnPointTransform(Vector3 position)
    {
        return _enemySpawnPoints.OrderBy(x => Vector3.Distance(x.position, position)).First();
    }
}