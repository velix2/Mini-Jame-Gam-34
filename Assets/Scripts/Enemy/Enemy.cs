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
    [SerializeField] private float knockBackStrength = 1f;
    [SerializeField] private Animator animator;


    private int _health = 3;
    private Rigidbody2D _rb;
    protected Vector2 _moveDirection;

    private bool _isMoving = false;
    protected bool IsMoving
    {
        get => _isMoving;
        set
        {
            _isMoving = value;
            animator.SetBool(Moving, value);
        }
    }
    protected Phase _phase = Phase.ApproachArea;
    protected Vector2 _workPosition;
    private float _workCompletion = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = maxHealth;
    }

    private void Start()
    {
        animator.SetBool(Moving, true);
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
        IsMoving = true;
        _moveDirection = (Vector2.zero - (Vector2)transform.position).normalized;

        if (Vector2.Distance(transform.position, Vector2.zero) < 5.0f)
        {
            _phase = Phase.MoveToField;
            Debug.Log("Moving to field");
        }
    }

    protected void SetMoveDirectionTowards(Vector2 target)
    {
        _moveDirection = (target - (Vector2)transform.position).normalized;
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

    private bool _hasTargetHarvestPosition = false;
    private Vector3Int _targetHarvestCellPosition;
    private Vector3 _targetHarvestWorldPosition;
    private static readonly int Moving = Animator.StringToHash("isMoving");

    private void Harvest()
    {
        if (!_hasTargetHarvestPosition)
        {
            Debug.Log("Harvesting");
            //TODO: Check for ripe tomatoes and proceed to harvest

            var pos = FieldHandler.Instance.GetRandomRipeSeedPosition();
            if (pos == -Vector3.one)
            {
                pos = FieldHandler.Instance.GetRandomDeadSeedPosition();
                if (pos == -Vector3.one)
                {
                    Debug.Log("No seeds to harvest");
                    _phase = Phase.ReturnToBase;
                    return;
                }
            }
            _targetHarvestWorldPosition = pos;
            _targetHarvestCellPosition = FieldHandler.Instance.WorldToCell(_targetHarvestWorldPosition);
            _hasTargetHarvestPosition = true;
        }

        if (!FieldHandler.Instance.IsSeedAtCellRipeOrDead(_targetHarvestCellPosition))
        {
            //Seed was removed, get new one
            _hasTargetHarvestPosition = false;
            return;
        }
        IsMoving = true;
        SetMoveDirectionTowards(_targetHarvestWorldPosition);
        
        if (Vector2.Distance(transform.position, _targetHarvestWorldPosition) < 0.1f)
        {
            FieldHandler.Instance.RemoveSeed(_targetHarvestCellPosition);
            
            //Now 50-50 chance to return to base or continue harvesting
            if (UnityEngine.Random.value > 0.5f)
            {
                _hasTargetHarvestPosition = false;
                return;
            }
            _phase = Phase.ReturnToBase;
        }

    }


    private void ReturnToBase()
    {
        IsMoving = true;
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
        if (!IsMoving)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        _rb.velocity = _moveDirection * (moveSpeed * Time.fixedDeltaTime * 16f);
        //Flip sprite based on movement direction
        if (_moveDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (_moveDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collision");
        var otherRb = other.rigidbody;
        Vector2 directionOtherToThis = (transform.position - other.transform.position).normalized;
        var thisAmount = Mathf.Abs(Vector2.Dot(_rb.velocity, directionOtherToThis));
        var otherAmount = Mathf.Abs(Vector2.Dot(otherRb.velocity, directionOtherToThis));


        //If the other object is moving faster, "knock back" this object

        transform.position += (Vector3)directionOtherToThis * otherAmount * 0.1f * knockBackStrength;

        //"Knock back" the other object
        other.transform.position += (Vector3)directionOtherToThis * -thisAmount * 0.1f * knockBackStrength;
    }

}