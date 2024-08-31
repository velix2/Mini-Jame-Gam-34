using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldHandler : MonoBehaviour
{
    [SerializeField] private Tilemap fieldMap;
    [SerializeField] private Tile fieldTilePrefab;
    [SerializeField] private GameObject seedPrefab;
    
    
    public static FieldHandler Instance;
    
    private readonly HashSet<Vector3Int> _fieldTiles = new HashSet<Vector3Int>();
    private readonly HashSet<Vector3Int> _reservedFieldTiles = new HashSet<Vector3Int>();
    private readonly Dictionary<Vector3Int, Seed> _seeds = new Dictionary<Vector3Int, Seed>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

    }
    
    public Vector3 CellToWorldCentered(Vector3Int cellPosition)
    {
        return fieldMap.CellToWorld(cellPosition) + fieldMap.layoutGrid.cellSize * 0.5f;
    }
    
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return fieldMap.WorldToCell(worldPosition);
    }
    
    public void CreateFieldTile(Vector3 worldPosition)
    {
        Vector3Int gridPosition = fieldMap.WorldToCell(worldPosition);
        fieldMap.SetTile(gridPosition, fieldTilePrefab);
        _fieldTiles.Add(gridPosition);
    }
    
    public void RemoveFieldTile(Vector3 worldPosition)
    {
        Vector3Int gridPosition = fieldMap.WorldToCell(worldPosition);
        fieldMap.SetTile(gridPosition, null);
        _fieldTiles.Remove(gridPosition);
    }
    
    public GameObject GetFieldTileAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int gridPosition = fieldMap.WorldToCell(worldPosition);
        return fieldMap.GetInstantiatedObject(gridPosition);
    }
    
    public GameObject GetFieldTileAtGridPosition(Vector3Int gridPosition)
    {
        return fieldMap.GetInstantiatedObject(gridPosition);
    }
    
    public Vector3 GetWorldPositionOfRandomFieldTile()
    {
        if (_fieldTiles.Count == 0)
        {
            return Vector3.zero;
        }
        int randomIndex = UnityEngine.Random.Range(0, _fieldTiles.Count);
        var cellCoords = _fieldTiles.ToList()[randomIndex];
        return CellToWorldCentered(cellCoords);
    }
    
    public bool DoFieldTilesExist()
    {
        return _fieldTiles.Count > 0;
    }
    
    public bool DoesEmptyPlowedFieldWithWorldCoordsExist(Vector3 worldPosition)
    {
        Vector3Int gridPosition = fieldMap.WorldToCell(worldPosition);
        return _fieldTiles.Contains(gridPosition) && !_seeds.ContainsKey(gridPosition);
    }
    
    public Vector3Int ReserveEmptyFieldTile()
    {
        Vector3Int randomPosition;
        do
        {
            randomPosition = GameAreaHandler.Instance.GetRandomCellPositionInsideGameArea();
        } while (_fieldTiles.Contains(randomPosition) || _reservedFieldTiles.Contains(randomPosition));
        
        _reservedFieldTiles.Add(randomPosition);
        return randomPosition;
    }
    
    public void UnreserveFieldTile(Vector3Int gridPosition)
    {
        _reservedFieldTiles.Remove(gridPosition);
    }
    
    public void PlantSeed(Vector3Int gridPosition)
    {
        if (_seeds.ContainsKey(gridPosition))
        {
            return;
        }
        GameObject seedObject = Instantiate(seedPrefab, CellToWorldCentered(gridPosition), Quaternion.identity);
        Seed seed = seedObject.GetComponent<Seed>();
        _seeds.Add(gridPosition, seed);
    }
    
    public Vector3 GetRandomRipeSeedPosition()
    {
        var ripeSeedsList = _seeds.Where(pair => pair.Value.GrowthStage is Seed.GrowthStages.Ripe).Select(pair => pair.Value).ToList();
        
        if (ripeSeedsList.Count == 0)
        {
            return -Vector3.one;
        }
        int randomIndex = UnityEngine.Random.Range(0, ripeSeedsList.Count);
        return ripeSeedsList[randomIndex].transform.position;
    }
    
    public Vector3 GetRandomDeadSeedPosition()
    {
        var deadSeedsList = _seeds.Where(pair => pair.Value.GrowthStage is Seed.GrowthStages.Dead).Select(pair => pair.Value).ToList();
        
        if (deadSeedsList.Count == 0)
        {
            return -Vector3.one;
        }
        int randomIndex = UnityEngine.Random.Range(0, deadSeedsList.Count);
        return deadSeedsList[randomIndex].transform.position;
    }
    
    public bool IsSeedAtCellRipeOrDead(Vector3Int gridPosition)
    {
        if (!_seeds.ContainsKey(gridPosition))
        {
            return false;
        }
        return _seeds[gridPosition].GrowthStage is Seed.GrowthStages.Ripe || _seeds[gridPosition].GrowthStage is Seed.GrowthStages.Dead;
    }
    
    public void RemoveSeed(Vector3Int gridPosition)
    {
        if (!_seeds.TryGetValue(gridPosition, out var seed))
        {
            return;
        }
        Destroy(seed.gameObject);
        _seeds.Remove(gridPosition);
    }
}
