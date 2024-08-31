using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Seed : MonoBehaviour
{
    public enum GrowthStages
    {
        Growing0,
        Growing1,
        Ripe,
        Dead,
    }

    [System.Serializable]
    public class SeedSprites
    {
        public Sprite stage1;
        public Sprite ripe;
        public Sprite dead;
    }

    [SerializeField] private SeedSprites sprites;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float wateredGrowthDurationMultiplier = 0.5f;
    
    [SerializeField] private float growthTime0To1 = 10f;
    [SerializeField] private float growthTime1ToRipe = 10f;
    [SerializeField] private float growthTimeRipeToDead = 20f;
    [SerializeField] private float maxDeviation = 5.0f;
    private GrowthStages _growthStage = GrowthStages.Growing0;
    private float _growthTimer = 0f;

    private float _growthTime0;
    private float _growthTime1;
    private float _growthTime2;

    private bool _isWatered = false;

    public bool IsWatered
    {
        get => _isWatered;
    }

    private void Awake()
    {
        _growthTime0 = growthTime0To1 + Random.Range(-maxDeviation, maxDeviation);
        _growthTime1 = growthTime1ToRipe + Random.Range(-maxDeviation, maxDeviation);
        _growthTime2 = growthTimeRipeToDead + Random.Range(-maxDeviation, maxDeviation);
    }
    
    //TODO: Make this more efficient
    private void Update()
    {
        _growthTimer += Time.deltaTime;
        if (_growthTimer >= _growthTime0 && _growthStage == GrowthStages.Growing0)
        {
            _growthStage = GrowthStages.Growing1;
            spriteRenderer.sprite = sprites.stage1;
            
            _growthTimer = 0f;
        }
        else if (_growthTimer >= _growthTime1 && _growthStage == GrowthStages.Growing1)
        {
            _growthStage = GrowthStages.Ripe;
            spriteRenderer.sprite = sprites.ripe;
            _growthTimer = 0f;
        }
        else if (_growthTimer >= _growthTime2 && _growthStage == GrowthStages.Ripe)
        {
            _growthStage = GrowthStages.Dead;
            spriteRenderer.sprite = sprites.dead;
            _growthTimer = 0f;
        }
    }
    
    public void Water()
    {
        if (_isWatered) return;
        _isWatered = true;
        _growthTime0 *= wateredGrowthDurationMultiplier;
    }

    public GrowthStages GrowthStage => _growthStage;
}
