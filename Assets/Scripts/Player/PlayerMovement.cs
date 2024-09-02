using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [System.Serializable]
    public class MoveSettings
    {
        public float moveSpeed = 12f;
        public float sprintMultiplier = 1.5f;
    }

    [System.Serializable]
    public class InteractionSettings
    {
        public float swordCooldown = 0.5f;
        public float trampleDurationInSecs = 1.0f;
    }

    [SerializeField] private MoveSettings moveSettings;
    [SerializeField] private InteractionSettings interactionSettings;
    [SerializeField] private ItemNearbyHandler itemNearbyHandler;
    [SerializeField] private EnemyNearbyHandler enemyNearbyHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private ProgressBarInWorld durabilityBar;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private ItemType _currentItemType = ItemType.None;
    private Item _currentItem;
    private bool _isOnCooldown = false;
    private bool _isTrampling = false;
    
    private static readonly int ItemId = Animator.StringToHash("itemId");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private float _baseAnimationSpeed;
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        _baseAnimationSpeed = animator.speed;
    }

    void Update()
    {
        PickUpDrop();
        Interact();
        Trample();

        PlayAudio();
    }

    private void PlayAudio()
    {
        if (_isTrampling || _rb.velocity.magnitude > 0.1f)
        {
            if (_audioSource.isPlaying) return;
            _audioSource.Play();
        } else
        {
            _audioSource.Stop();
        }
    }


    private void Interact()
    {
        if (!GameHandler.Instance.IsWaveInProgress) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        if (_currentItemType == ItemType.None) return;
        UseItem();
    }

    private void UseItem()
    {
        if (_isOnCooldown)
        {
            return;
        }

        switch (_currentItemType)
        {
            case ItemType.Weapon:
                AttackEnemies();
                GoOnCooldown(interactionSettings.swordCooldown);
                DecreaseDurability();
                break;
            case ItemType.Poison:
                UsePoison();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DecreaseDurability()
    {
        _currentItem.Durability--;
        durabilityBar.SetValue(_currentItem.Durability);
        if (_currentItem.Durability > 0) return;
        durabilityBar.SetVisible(false);
        animator.SetInteger(ItemId, 0);
        Destroy(_currentItem.gameObject);
        _currentItemType = ItemType.None;
        _currentItem = null;
    }

    private void PickUpDrop()
    {
        if (_isOnCooldown)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (_currentItemType == ItemType.None)
        {
            Debug.Log("Looking for item");
            var itemType = itemNearbyHandler.GetItemTypeNearby();
            if (itemType != ItemType.None)
            {
                _currentItemType = itemType;
                _currentItem = itemNearbyHandler.GetItemNearby();
                _currentItem.PickUp(gameObject);
                durabilityBar.SetMaxValue(_currentItem.MaxDurability);
                durabilityBar.SetValue(_currentItem.Durability);
                durabilityBar.SetVisible(true);

                if (_currentItemType == ItemType.Poison)
                {
                    itemNearbyHandler.UseSeedHighlighting = true;
                }

                Debug.Log("Picked up item: " + _currentItemType);
            }
        }
        else
        {
            Debug.Log("Dropped item: " + _currentItemType);
            if (_currentItemType == ItemType.Poison)
            {
                itemNearbyHandler.UseSeedHighlighting = false;
            }
            _currentItem.Drop();
            durabilityBar.SetVisible(false);
            _currentItemType = ItemType.None;
            _currentItem = null;
        }

        //Animator
        animator.SetInteger(ItemId, (int)_currentItemType);
    }

    public void TakeAwayItem()
    {
        if (_currentItemType == ItemType.None) return;
        
        durabilityBar.SetVisible(false);
        animator.SetInteger(ItemId, 0);
        Destroy(_currentItem.gameObject);
        _currentItemType = ItemType.None;
        _currentItem = null;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Basic movement
        if (_isTrampling)
        {
            _rb.velocity = Vector2.zero;
            animator.SetBool(IsWalking, false);
            return;
        }
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _moveInput = Vector2.ClampMagnitude(_moveInput, 1);
        animator.speed = _baseAnimationSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _rb.velocity = _moveInput *
                           (moveSettings.moveSpeed * moveSettings.sprintMultiplier * Time.fixedDeltaTime * 16f);
            animator.speed *= moveSettings.sprintMultiplier;
            //_rb.AddForce(_moveInput * (moveSettings.moveSpeed * moveSettings.sprintMultiplier * Time.fixedDeltaTime * 16f));
        }
        else
        {
            _rb.velocity = _moveInput * (moveSettings.moveSpeed * Time.fixedDeltaTime * 16f);
            //_rb.AddForce(_moveInput * (moveSettings.moveSpeed * Time.fixedDeltaTime * 16f));
        }

        //Flip sprite based on movement direction
        if (_moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (_moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //Animator
        animator.SetBool(IsWalking, _moveInput.magnitude > 0.1f);
    }

    private void AttackEnemies()
    {
        var enemies = enemyNearbyHandler.GetEnemiesNearby();
        animator.SetBool(IsAttacking, true);
        _currentItem.PlayUseSound();
        StartCoroutine(DisableIsAttacking());
        foreach (var enemy in enemies)
        {
            enemy.Damage(1);
        }
    }

    private IEnumerator DisableIsAttacking()
    {
        yield return new WaitForSeconds(interactionSettings.swordCooldown / 2f);
        animator.SetBool(IsAttacking, false);
    }
    
    private void UsePoison()
    {
        var seed = itemNearbyHandler.GetSeedNearby();
        if (!seed) return;
        _currentItem.PlayUseSound();
        seed.Poison();
        GoOnCooldown(1.0f);
        DecreaseDurability();
    }

    private void GoOnCooldown(float durationInSecs)
    {
        _isOnCooldown = true;
        StartCoroutine(EndCooldown(durationInSecs));
    }

    private IEnumerator EndCooldown(float durationInSecs)
    {
        yield return new WaitForSeconds(durationInSecs);
        _isOnCooldown = false;
    }
    
    private Coroutine _trampleCoroutine;
    private static readonly int IsTrampling = Animator.StringToHash("isTrampling");
    private AudioSource _audioSource;

    private void Trample()
    {
        if (!GameHandler.Instance.IsWaveInProgress)
        {
            if (!_isTrampling) return;
            _isTrampling = false;
            animator.SetBool(IsTrampling, false);
            StopCoroutine(_trampleCoroutine);
            return;
        }

        if (_rb.velocity.magnitude < 0.1f && _currentItemType == ItemType.None && !_isTrampling && Input.GetKeyDown(KeyCode.Q))
        {
            _isTrampling = true;
            animator.SetBool(IsTrampling, true);
            _trampleCoroutine = StartCoroutine(AwaitTrampleEnd());
        }
        else if (_isTrampling && Input.GetKeyUp(KeyCode.Q))
        {
            _isTrampling = false;
            animator.SetBool(IsTrampling, false);

            StopCoroutine(_trampleCoroutine);
        }
    }

    private IEnumerator AwaitTrampleEnd()
    {
        yield return new WaitForSeconds(interactionSettings.trampleDurationInSecs);
        _isTrampling = false;
        OnTrampleComplete();
    }

    private void OnTrampleComplete()
    {
        var footPosition = transform.position + new Vector3(0, -1f, 0);
        FieldHandler.Instance.TrampleFieldAtWorldPos(footPosition);
        animator.SetBool(IsTrampling, false);
    }
}