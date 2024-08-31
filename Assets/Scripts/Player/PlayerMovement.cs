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
    private static readonly int ItemId = Animator.StringToHash("itemId");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private float _baseAnimationSpeed;
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    // Start is called before the first frame update
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _baseAnimationSpeed = animator.speed;
    }

    // Update is called once per frame
    void Update()
    {
        PickUpDrop();
        Interact();
        //DebugMethod();
    }

    private void DebugMethod()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            FieldHandler.Instance.CreateFieldTile(transform.position);
        }
    }

    private void Interact()
    {
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
                DecreaseDurability();
                GoOnCooldown(interactionSettings.swordCooldown);
                break;
            case ItemType.Poison:
                DecreaseDurability();
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
        //TODO: Animation switches too early
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

                Debug.Log("Picked up item: " + _currentItemType);
            }
        }
        else
        {
            Debug.Log("Dropped item: " + _currentItemType);
            _currentItem.Drop();
            durabilityBar.SetVisible(false);
            _currentItemType = ItemType.None;
            _currentItem = null;
        }

        //Animator
        animator.SetInteger(ItemId, (int)_currentItemType);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Basic movement
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
}