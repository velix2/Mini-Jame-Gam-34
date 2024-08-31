using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldHandler : MonoBehaviour
{
    [SerializeField] private Tilemap fieldMap;
    [SerializeField] private Tile fieldTilePrefab;
    
    public static FieldHandler Instance;
    
    private HashSet<Vector3Int> _fieldTiles = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> _reservedFieldTiles = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        return fieldMap.CellToWorld(cellPosition);
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
        return fieldMap.CellToWorld(cellCoords);
    }
    
    public bool DoFieldTilesExist()
    {
        return _fieldTiles.Count > 0;
    }
    
    public bool DoesFieldWithWorldCoordsExist(Vector3 worldPosition)
    {
        Vector3Int gridPosition = fieldMap.WorldToCell(worldPosition);
        return _fieldTiles.Contains(gridPosition);
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
    
}
