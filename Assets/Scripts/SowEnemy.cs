using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SowEnemy : Enemy
{
    
    private bool _hasTargetPosition = false;
    private Vector3 _targetPosition;

    //TODO: Create Reservation Feature here as well
    protected override void Dispose()
    {
        
    }

    protected override void MoveToField()
    {
        //Reached target position
        if (Vector2.Distance(transform.position, _targetPosition) < 0.1f)
        {
            _isMoving = false;
            _phase = Phase.Work;
            _workPosition = _targetPosition;
            Debug.Log("Working");
            return;
        }
        
        //Get new target position
        if (!_hasTargetPosition)
        {
            //If no field tiles exist, wander
            if (!FieldHandler.Instance.DoFieldTilesExist())
            {
                _phase = Phase.Wander;
                Debug.Log("Wandering");
                return;
            }
            
            _targetPosition = FieldHandler.Instance.GetWorldPositionOfRandomFieldTile();
            _hasTargetPosition = true;
        }
        
        //Does Target still exist?
        else if (FieldHandler.Instance.DoesFieldWithWorldCoordsExist(_targetPosition))
        {
            _hasTargetPosition = false;
            return;
        }
        
        _isMoving = true;
        SetMoveDirectionTowards(_targetPosition);
    }
    

    //Wander to random position until field tiles exist
    protected override void Wander()
    {
        _isMoving = true;
        _moveDirection = ((Vector2)GameAreaHandler.Instance.GetRandomWorldPositionInsideGameArea() - (Vector2) transform.position).normalized;

        if (!FieldHandler.Instance.DoFieldTilesExist()) return;
        _phase = Phase.MoveToField;
        Debug.Log("Moving to field");
    }

    protected override void OnWorkCompleted()
    {
        
    }
    
}
