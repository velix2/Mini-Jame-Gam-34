using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SowEnemy : Enemy
{
    
    private bool _hasTargetPosition = false;
    private Vector3 _targetPosition;
    
    protected override void Dispose()
    {
        
    }

    protected override void MoveToField()
    {
        //Reached target position
        if (Vector2.Distance(transform.position, _targetPosition) < 0.1f)
        {
            IsMoving = false;
            _phase = Phase.Work;
            WorkPosition = _targetPosition;
            Debug.Log("Working");
            return;
        }
        
        //Get new target position
        if (!_hasTargetPosition)
        {
            //If no field tiles exist, wander
            if (!FieldHandler.Instance.DoEmptyPlowedFieldsExist())
            {
                _phase = Phase.Wander;
                _hasTargetPosition = false;
                Debug.Log("Wandering");
                return;
            }
            
            _targetPosition = FieldHandler.Instance.GetWorldPositionOfRandomFieldTile();
            _hasTargetPosition = true;
        }
        
        //Does Target still exist?
        else if (!FieldHandler.Instance.DoesEmptyPlowedFieldWithWorldCoordsExist(_targetPosition))
        {
            Debug.Log("Target does not exist");
            _hasTargetPosition = false;
            return;
        }
        
        IsMoving = true;
        SetMoveDirectionTowards(_targetPosition);
    }
    
    protected override bool WanderEndCondition => FieldHandler.Instance.DoEmptyPlowedFieldsExist();
    
    
    protected override void OnWorkCompleted()
    {
        FieldHandler.Instance.PlantSeed(FieldHandler.Instance.WorldToCell(WorkPosition));
    }
    
    
}
