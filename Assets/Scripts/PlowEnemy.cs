using UnityEngine;

public class PlowEnemy : Enemy
{
    private bool _hasFieldReserved = false;
    private Vector3 _fieldWorldPosition;
    private Vector3Int _fieldCellPosition;
    
    protected override void MoveToField()
    {
        if (!_hasFieldReserved)
        {
            _fieldCellPosition = FieldHandler.Instance.ReserveEmptyFieldTile();
            _fieldWorldPosition = FieldHandler.Instance.CellToWorld(_fieldCellPosition);
            _hasFieldReserved = true;
        }
        
        if (Vector2.Distance(transform.position, _fieldWorldPosition) < 0.1f)
        {
            _isMoving = false;
            _phase = Phase.Work;
            _workPosition = _fieldWorldPosition;
            return;
        }
        
        _isMoving = true;
        SetMoveDirectionTowards(_fieldWorldPosition);
    }

    protected override void Wander()
    {
        
    }

    protected override void OnWorkCompleted()
    {
        Debug.Log("Plowing completed");
        FieldHandler.Instance.CreateFieldTile(_fieldWorldPosition);
        _hasFieldReserved = false;
        UnreserveField();
    }

    protected override void Dispose()
    {
        UnreserveField();
    }
    
    private void UnreserveField()
    {
        FieldHandler.Instance.UnreserveFieldTile(_fieldCellPosition);
    }
}