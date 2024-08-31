using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNearbyHandler : MonoBehaviour
{
    private Item _currentItemNearby;
    private Seed _currentSeedNearby;
    
    private bool _useSeedHighlighting = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            Debug.Log("Item nearby: " + item.ItemType);
            if (_currentItemNearby != null)
            {
                _currentItemNearby.Unhighlight();
            }
            _currentItemNearby = item;
            _currentItemNearby.Highlight();
        } else if (other.CompareTag("Seed"))
        {
            Seed seed = other.GetComponent<Seed>();
            _currentSeedNearby = seed;
            if (UseSeedHighlighting)
            {
                _currentSeedNearby.Highlight();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item") && _currentItemNearby && other == _currentItemNearby.GetComponent<Collider2D>())
        {
            Debug.Log("Item not nearby");
            _currentItemNearby.Unhighlight();
            _currentItemNearby = null;
        }
        else if (other.CompareTag("Seed") && _currentSeedNearby && other == _currentSeedNearby.GetComponent<Collider2D>())
        {
            if (UseSeedHighlighting)
            {
                _currentSeedNearby.Unhighlight();
            }
            _currentSeedNearby = null;
        }
    }
    
    public ItemType GetItemTypeNearby()
    {
        return _currentItemNearby != null ? _currentItemNearby.ItemType : ItemType.None;
    }
    
    public Item GetItemNearby()
    {
        return _currentItemNearby;
    }

    public Seed GetSeedNearby()
    {
        return _currentSeedNearby;
    }

    public bool UseSeedHighlighting
    {
        set
        {
            _useSeedHighlighting = value;
            if (_currentSeedNearby == null) return;
            if (value)
            {
                _currentSeedNearby.Highlight();
            }
            else
            {
                _currentSeedNearby.Unhighlight();
            }
        }
        get => _useSeedHighlighting;
    }
}
