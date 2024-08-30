using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNearbyHandler : MonoBehaviour
{
    private readonly List<Enemy> _enemiesNearby = new List<Enemy>();
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        Enemy enemy = other.GetComponent<Enemy>();
        if (!_enemiesNearby.Contains(enemy))
        {
            _enemiesNearby.Add(enemy);
            Debug.Log("Add: " + _enemiesNearby.Count);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        Enemy enemy = other.GetComponent<Enemy>();
        _enemiesNearby.Remove(enemy);
        Debug.Log("Remove: " + _enemiesNearby.Count);
    }
    
    public List<Enemy> GetEnemiesNearby()
    {
        return _enemiesNearby;
    }
}
