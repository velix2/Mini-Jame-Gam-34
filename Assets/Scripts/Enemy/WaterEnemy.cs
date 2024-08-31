using UnityEngine;

public class WaterEnemy : Enemy
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
            _workPosition = _targetPosition;
            Debug.Log("Working");
            return;
        }
        
        //Get new target position
        if (!_hasTargetPosition)
        {
            //If no field tiles exist, wander
            if (!FieldHandler.Instance.DoUnwateredTilesExist())
            {
                _phase = Phase.Wander;
                _hasTargetPosition = false;
                Debug.Log("Wandering");
                return;
            }
            
            _targetPosition = FieldHandler.Instance.GetWorldPositionOfRandomUnwateredTile();
            _hasTargetPosition = true;
        }
        
        //Does Target still exist?
        else if (!FieldHandler.Instance.DoesUnwateredFieldWithWorldCoordsExist(_targetPosition))
        {
            Debug.Log("Target does not exist");
            _hasTargetPosition = false;
            return;
        }
        
        IsMoving = true;
        SetMoveDirectionTowards(_targetPosition);
    }

    protected override void Wander()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnWorkCompleted()
    {
        throw new System.NotImplementedException();
    }
    
}