using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        Plow,
        Sow,
        Water,
    }
    
    public enum Phase
    {
        ApproachArea,
        MoveToField,
        Wander,
        Work,
        Harvest,
        ReturnToBase,
    }
    
    [SerializeField] private Type enemyType;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Renderer spriteRenderer;

    private int _health = 3;
    private Rigidbody2D _rb;
    private Vector2 _moveDirection;
    private bool _isMoving;
    private Phase _phase = Phase.ApproachArea;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = maxHealth;
    }

    public void Damage(int damage)
    {
        _health -= damage;
        IndicateDamage();
    }
    
    private void IndicateDamage()
    {
        StartCoroutine(FlashRedOnDamage());
        //TODO. Add particles
    }
    
    private IEnumerator FlashRedOnDamage()
    {
        spriteRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.material.color = Color.white;
    }
    
    

    private void Kill()
    {
        if (_health > 0) return;
        Destroy(gameObject);
    }

    private void Update()
    {
        Act();
        Kill();
    }

    private void Act()
    {
        switch (_phase)
        {
            case Phase.ApproachArea:
                ApproachArea();
                break;
            case Phase.MoveToField:
                MoveToField();
                break;
            case Phase.Wander:
                Wander();
                break;
            case Phase.Work:
                Work();
                break;
            case Phase.Harvest:
                break;
            case Phase.ReturnToBase:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //TODO: Actually consider the area
    private void ApproachArea()
    {
        _isMoving = true;
        _moveDirection = (Vector2.zero - (Vector2) transform.position).normalized;
        
        if (Vector2.Distance(transform.position, Vector2.zero) < 5.0f)
        {
            _phase = Phase.MoveToField;
            Debug.Log("Moving to field");
        }
    }
    
    private bool _hasTargetPosition = false;
    private Vector3 _targetPosition;
    private void MoveToField()
    {
        //Reached target position
        if (Vector2.Distance(transform.position, _targetPosition) < 0.1f)
        {
            _isMoving = false;
            _hasTargetPosition = false;
            _phase = Phase.Work;
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
        _moveDirection = ((Vector2)_targetPosition - (Vector2) transform.position).normalized;
    }

    private void Wander()
    {
        _isMoving = true;
        _moveDirection = ((Vector2)GameAreaHandler.Instance.GetRandomWorldPositionInsideGameArea() - (Vector2) transform.position).normalized;

        if (!FieldHandler.Instance.DoFieldTilesExist()) return;
        _phase = Phase.MoveToField;
        Debug.Log("Moving to field");
    }

    private void Work()
    {
        if (Vector2.Distance(transform.position, _targetPosition) > 0.1f)
        {
            _phase = Phase.MoveToField;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }
    
    private void Move()
    {
        if (!_isMoving)
        {
            _rb.velocity = Vector2.zero;
            return;
        }
        _rb.velocity = _moveDirection * (moveSpeed * Time.fixedDeltaTime * 16f); 
        
    }
    
}
