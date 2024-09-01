using System;
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
            ReserveField();
        }
        
        if (Vector2.Distance(transform.position, _fieldWorldPosition) < 0.1f)
        {
            IsMoving = false;
            _phase = Phase.Work;
            WorkPosition = _fieldWorldPosition;
            return;
        }
        
        IsMoving = true;
        SetMoveDirectionTowards(_fieldWorldPosition);
    }

    private void ReserveField()
    {
        _fieldCellPosition = FieldHandler.Instance.ReserveEmptyFieldTile();
        _fieldWorldPosition = FieldHandler.Instance.CellToWorldCentered(_fieldCellPosition);
        _hasFieldReserved = true;
    }

    protected override bool WanderEndCondition => true;

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