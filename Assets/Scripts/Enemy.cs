using System;
using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public enum Phase
    {
        ApproachArea,
        MoveToField,
        Wander,
        Work,
        Harvest,
        ReturnToBase,
    }
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Renderer spriteRenderer;
    [SerializeField] private float workSpeed = 1f;
    

    private int _health = 3;
    private Rigidbody2D _rb;
    protected Vector2 _moveDirection;
    protected bool _isMoving;
    protected Phase _phase = Phase.ApproachArea;
    protected Vector2 _workPosition;
    private float _workCompletion = 0f;

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
        Dispose();
        Destroy(gameObject);
    }

    protected abstract void Dispose();

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
                Harvest();
                break;
            case Phase.ReturnToBase:
                ReturnToBase();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //TODO: Actually consider the area
    private void ApproachArea()
    {
        _isMoving = true;
        _moveDirection = (Vector2.zero - (Vector2)transform.position).normalized;

        if (Vector2.Distance(transform.position, Vector2.zero) < 5.0f)
        {
            _phase = Phase.MoveToField;
            Debug.Log("Moving to field");
        }
    }
    
    protected void SetMoveDirectionTowards(Vector2 target)
    {
        _moveDirection = (target - (Vector2) transform.position).normalized;
    }
    
    protected abstract void MoveToField();
    
    protected abstract void Wander();
    
    private void Work()
    {
        if (Vector2.Distance(transform.position, _workPosition) > 0.1f)
        {
            Debug.Log("Returning to work position");
            _phase = Phase.MoveToField;
            return;
        }
        Debug.Log("Working, completion: " + _workCompletion);
        ProgressWork();
        if (_workCompletion < 1f) return;
        OnWorkCompleted();
        _phase = Phase.Harvest;
    }

    protected abstract void OnWorkCompleted();

    private void ProgressWork()
    {
        _workCompletion += Time.deltaTime * workSpeed;
    }
    
    private void Harvest()
    {
        Debug.Log("Harvesting");
        //TODO: Check for ripe tomatoes and proceed to harvest
        //For now, just return to base
        _phase = Phase.ReturnToBase;
    }
    
    
    private void ReturnToBase()
    {
        _isMoving = true;
        SetMoveDirectionTowards(Vector2.left * 15f);

        if (Vector2.Distance(transform.position, Vector2.left * 15f) < 0.1f)
        {
            Dispose();
            Destroy(gameObject);
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