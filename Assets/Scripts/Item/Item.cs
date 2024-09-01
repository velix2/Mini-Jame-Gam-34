using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteHighlighted;
    [SerializeField] private int maxDurability = 8;
    private Collider2D _collider;
    private Sprite _spriteDefault;

    public bool IsInSpawner { get; set; }

    public virtual void Awake()
    {
        Debug.Log("Item awake");
        _collider = GetComponent<Collider2D>();
        _spriteDefault = spriteRenderer.sprite;
        Durability = maxDurability;
    }

    protected ItemType itemType;
    
    public ItemType ItemType => itemType;

    public int Durability { get; set; }

    public int MaxDurability => maxDurability;

    public void PickUp(GameObject itemHolder)
    {
        IsInSpawner = false;
        
        spriteRenderer.enabled = false;
        _collider.enabled = false;
        
        transform.SetParent(itemHolder.transform);
        transform.localPosition = Vector3.right * 0.5f;
    }
    
    public void Drop()
    {
        _collider.enabled = true;
        spriteRenderer.enabled = true;
        transform.SetParent(null);
    }
    
    public void Highlight()
    {
        spriteRenderer.sprite = spriteHighlighted;
    }
    
    public void Unhighlight()
    {
        spriteRenderer.sprite = _spriteDefault;
    }
}