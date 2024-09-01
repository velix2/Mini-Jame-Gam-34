using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private ProgressBarInWorld progressBarInWorld;
    [SerializeField] private int[] itemsPerWave;
    private int _numberOfItemsCurrentWave;
    private int _numberOfItemsSpawned;
    
    private Item _currentItem;

    public void OnPrepPhase()
    {
        StopAllCoroutines();
        progressBarInWorld.SetVisible(false);
        
        _numberOfItemsCurrentWave = itemsPerWave[Math.Clamp(GameHandler.Instance.CurrentWave, 0, itemsPerWave.Length - 1)];
        _numberOfItemsSpawned = 0;
        progressBarInWorld.SetMaxValue(_numberOfItemsCurrentWave);
        progressBarInWorld.SetValue(_numberOfItemsCurrentWave);
        CreateItem();
    }

    public void OnWaveStart()
    {
        progressBarInWorld.SetVisible(true);
        StartCoroutine(SpawnItems());
    }


    private IEnumerator SpawnItems()
    {
        while (_numberOfItemsSpawned < _numberOfItemsCurrentWave)
        {
            yield return new WaitUntil(() => !_currentItem.IsInSpawner);
            CreateItem();
            progressBarInWorld.SetValue(_numberOfItemsCurrentWave - _numberOfItemsSpawned);
        }
    }
    

    private void CreateItem()
    {
        //If there is an item in the spawner, don't create a new one
        if (_currentItem && _currentItem.IsInSpawner) return;
        Debug.Log("Creating item");
        var item = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<Item>();
        item.IsInSpawner = true;
        _currentItem = item;
        _numberOfItemsSpawned++;
        GameHandler.Instance.ReportItemSpawn(item);
    }
    
}
