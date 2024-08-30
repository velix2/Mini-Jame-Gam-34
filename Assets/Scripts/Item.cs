using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteHighlighted;
    private Collider2D _collider;
    private Sprite _spriteDefault;

    public virtual void Awake()
    {
        Debug.Log("Item awake");
        _collider = GetComponent<Collider2D>();
        _spriteDefault = spriteRenderer.sprite;
    }

    protected ItemType itemType;
    private int durablity;

    public ItemType ItemType => itemType;

    public int Durablity
    {
        get => durablity;
        set => durablity = value;
    }
    
    public void PickUp(GameObject itemHolder)
    {
        spriteRenderer.enabled = false;
        _collider.enabled = false;
        
        transform.SetParent(itemHolder.transform);
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