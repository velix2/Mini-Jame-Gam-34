using System;
using UnityEngine;

public class GameAreaHandler : MonoBehaviour
{
    [SerializeField] private Vector3Int topLeftCorner;
    [SerializeField] private Vector3Int bottomRightCorner;
    
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
    }

    private void Start()
    {
        _topLeftCornerWorldPosition = FieldHandler.Instance.GetFieldTileAtGridPosition(topLeftCorner).transform.position;
        _bottomRightCornerWorldPosition = FieldHandler.Instance.GetFieldTileAtGridPosition(bottomRightCorner).transform.position;
    }
    
    public bool IsPositionInsideGameArea(Vector3 position)
    {
        return position.x >= _topLeftCornerWorldPosition.x && position.x <= _bottomRightCornerWorldPosition.x &&
               position.y >= _bottomRightCornerWorldPosition.y && position.y <= _topLeftCornerWorldPosition.y;
    }
    
    public Vector3 GetRandomWorldPositionInsideGameArea()
    {
        float randomX = UnityEngine.Random.Range(_topLeftCornerWorldPosition.x, _bottomRightCornerWorldPosition.x);
        float randomY = UnityEngine.Random.Range(_bottomRightCornerWorldPosition.y, _topLeftCornerWorldPosition.y);
        return new Vector3(randomX, randomY, 0);
    }
}