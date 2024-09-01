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
            WorkPosition = _targetPosition;
            Debug.Log("Working");
            return;
        }
        
        //Get new target position
        if (!_hasTargetPosition)
        {
            //If no field tiles exist, go to harvest
            if (!FieldHandler.Instance.DoUnwateredTilesExist())
            {
                _phase = Phase.Harvest;
                _hasTargetPosition = false;
                Debug.Log("Harvesting");
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
        
    }

    protected override void OnWorkCompleted()
    {
        Debug.Log("Watering completed");
        FieldHandler.Instance.WaterSeed(FieldHandler.Instance.WorldToCell(WorkPosition));
    }
    
}