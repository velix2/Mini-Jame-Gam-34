using System;
using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected enum Phase
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

    private static readonly int Moving = Animator.StringToHash("isMoving");
    private static readonly int IsWorking = Animator.StringToHash("isWorking");
    
    private int _health = 3;
    private Rigidbody2D _rb;
    private EnemyVisibilityNotifier _evn;
    protected Vector2 MoveDirection;

    private bool _isMoving;
    protected bool IsMoving
    {
        get => _isMoving;
        set
        {
            _isMoving = value;
            animator.SetBool(Moving, value);
        }
    }
    // ReSharper disable once InconsistentNaming
    protected Phase _phase = Phase.ApproachArea;
    protected Vector2 WorkPosition;
    private float _workCompletion;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _evn = spriteRenderer.GetComponent<EnemyVisibilityNotifier>();
        _health = maxHealth;
    }

    private void Start()
    {
        animator.SetBool(Moving, true);
    }

    public void Damage(int damage)
    {
        _health -= damage;
        ScoreHandler.Instance.Score += ScoreHandler.Instance.scoreValues.damage;
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
        ScoreHandler.Instance.Score += ScoreHandler.Instance.scoreValues.kill;
        Dispose();
        ReturnMarker();
        Destroy(gameObject);
        GameHandler.Instance.EnemyRemoved(this);
    }

    protected abstract void Dispose();

    private void Update()
    {
        Act();
        Kill();
        
        ShowHideMarker();
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
    private bool _hasEntryPosition;
    private Vector3 _entryPosition;
    private void ApproachArea()
    {
        if (!_hasEntryPosition)
        {
            _entryPosition = GameAreaHandler.Instance.GetClosestEntryPointPosition(transform.position);
            _hasEntryPosition = true;
        }
        IsMoving = true;
        SetMoveDirectionTowards(_entryPosition);

        if (!(Vector2.Distance(transform.position, _entryPosition) < 0.1f)) return;
        _phase = Phase.MoveToField;
        Debug.Log("Moving to field");
    }

    protected void SetMoveDirectionTowards(Vector2 target)
    {
        MoveDirection = (target - (Vector2)transform.position).normalized;
    }

    protected abstract void MoveToField();
    
    private Vector3 _wanderTargetPosition;
    private bool _hasWanderTargetPosition;
    [SerializeField] private float maxWanderDurationInSeconds = 5f;
    private float _wanderDuration;

    private void Wander()
    {
        _wanderDuration += Time.deltaTime;
        if (_wanderDuration > maxWanderDurationInSeconds)
        {
            _phase = Phase.Harvest;
            Debug.Log("Go To Harvest");
            return;
        }
        IsMoving = true;
        
        if (Vector3.Distance(_wanderTargetPosition, transform.position) < 0.1f)
        {
            _hasWanderTargetPosition = false;
            
        }
        
        if (!_hasWanderTargetPosition)
        {
            _wanderTargetPosition = GameAreaHandler.Instance.GetRandomWorldPositionInsideGameArea();
            _hasWanderTargetPosition = true;
        }
        MoveDirection = ((Vector2)_wanderTargetPosition - (Vector2) transform.position).normalized;

        if (!WanderEndCondition) return;
        _phase = Phase.MoveToField;
        _hasWanderTargetPosition = false;
        Debug.Log("Moving to field");
    }

    protected abstract bool WanderEndCondition
    {
        get;
    }

    private void Work()
    {
        if (Vector2.Distance(transform.position, WorkPosition) > 0.1f)
        {
            Debug.Log("Returning to work position");
            animator.SetBool(IsWorking, false);
            _phase = Phase.MoveToField;
            return;
        }
        animator.SetBool(IsWorking, true);

        Debug.Log("Working, completion: " + _workCompletion);
        ProgressWork();
        if (_workCompletion < 1f) return;
        OnWorkCompleted();
        animator.SetBool(IsWorking, false);
        _phase = Phase.Harvest;
    }

    protected abstract void OnWorkCompleted();

    private void ProgressWork()
    {
        _workCompletion += Time.deltaTime * workSpeed;
    }

    private bool _hasTargetHarvestPosition;
    private Vector3Int _targetHarvestCellPosition;
    private Vector3 _targetHarvestWorldPosition;

    private void Harvest()
    {
        if (!_hasTargetHarvestPosition)
        {
            Debug.Log("Harvesting");

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
    
    private bool _hasExitPosition;
    private Vector3 _exitPosition;
    private bool _isMovingToDespawn;
    private void ReturnToBase()
    {
        if (!_isMovingToDespawn)
        {
            if (!_hasExitPosition)
            {
                _exitPosition = GameAreaHandler.Instance.GetClosestEntryPointPosition(transform.position);
                _hasExitPosition = true;
            }

            IsMoving = true;
            SetMoveDirectionTowards(_exitPosition);

            if (!(Vector2.Distance(transform.position, _exitPosition) < 0.1f)) return;
            _isMovingToDespawn = true;
            _exitPosition = GameAreaHandler.Instance.GetClosestSpawnPointPosition(_exitPosition);
        }
        else
        {
            IsMoving = true;
            SetMoveDirectionTowards(_exitPosition);
            
            if (!(Vector2.Distance(transform.position, _exitPosition) < 0.1f)) return;
            Dispose();
            GameHandler.Instance.EnemyRemoved(this);
            ReturnMarker();
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

        _rb.velocity = MoveDirection * (moveSpeed * Time.fixedDeltaTime * 16f);
        transform.localScale = MoveDirection.x switch
        {
            //Flip sprite based on movement direction
            > 0 => new Vector3(1, 1, 1),
            < 0 => new Vector3(-1, 1, 1),
            _ => transform.localScale
        };
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
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

    private bool IsVisible => _evn.IsVisible;

    #region Enemy Marker

    private MarkerScript _marker;
    public bool HasMarker { get; private set; }

    public void AssignMarker(MarkerScript marker)
    {
        _marker = marker;
        marker.SetTarget(transform);
        _marker.gameObject.SetActive(true);
        HasMarker = true;
    }

    private void ReturnMarker()
    {
        if (!HasMarker)
        {
            return;
        }
        HasMarker = false;
        GameHandler.Instance.ReturnMarkerToPool(_marker);
        _marker = null;
    }

    private void ShowHideMarker()
    {
        if (HasMarker)
        {
            _marker.gameObject.SetActive(!IsVisible);
        }
    } 

    #endregion
    
}