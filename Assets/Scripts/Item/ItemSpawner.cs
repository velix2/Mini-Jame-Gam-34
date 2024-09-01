using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private float spawnCooldownInSeconds = 20f;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private ProgressBarInWorld progressBarInWorld;
    
    private bool _isProductionActive;

    private Item _currentItem;

    private void Start()
    {
        CreateItem();
    }

    private void Update()
    {
        if (GameHandler.Instance.IsWaveInProgress)
        {
            if(_isProductionActive) return;
            _isProductionActive = true;
             StartCoroutine(SpawnItems());
        }
        else
        {
            if(!_isProductionActive) return;
            _isProductionActive = false;
            StopAllCoroutines();
            progressBarInWorld.SetVisible(false);
            CreateItem();
        }
    }

    private IEnumerator SpawnItems()
    {
        while (_isProductionActive)
        {
            yield return new WaitUntil(() => !_currentItem.IsInSpawner);
            AnimateProgressBar();
            yield return new WaitForSeconds(spawnCooldownInSeconds);
            CreateItem();
        }
    }

    private void AnimateProgressBar()
    {
        progressBarInWorld.SetVisible(true);
        progressBarInWorld.SetMaxValue(spawnCooldownInSeconds);
        progressBarInWorld.SetValue(0f);
        StartCoroutine(ProgressBarAnimation());
    }
    
    private IEnumerator ProgressBarAnimation()
    {
        float time = 0f;
        while (time < spawnCooldownInSeconds)
        {
            progressBarInWorld.SetValue(time);
            time += Time.deltaTime;
            yield return null;
        }
        progressBarInWorld.SetVisible(false);
    }

    private void CreateItem()
    {
        //If there is an item in the spawner, don't create a new one
        if (_currentItem && _currentItem.IsInSpawner) return;
        var item = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<Item>();
        item.IsInSpawner = true;
        _currentItem = item;
        GameHandler.Instance.ReportItemSpawn(item);
    }
    
}
