using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNearbyHandler : MonoBehaviour
{
    private Item _currentItemNearby;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            Debug.Log("Item nearby: " + item.ItemType);
            _currentItemNearby = item;
            _currentItemNearby.Highlight();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item") && other == _currentItemNearby.GetComponent<Collider2D>())
        {
            Debug.Log("Item not nearby");
            _currentItemNearby.Unhighlight();
            _currentItemNearby = null;
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
}
