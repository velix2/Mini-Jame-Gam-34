using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ScoreHandler : MonoBehaviour
{
    [System.Serializable]
    public class ScoreValues
    {
        public int damage = 500;
        public int kill = 2000;
        public int poison = 1000;
        public int trample = 500;
        public int waveComplete = 10_000;
        public int bonusPerWave = 500;
    }
    
    public ScoreValues scoreValues;
    
    public static ScoreHandler Instance;
    [SerializeField] private TMP_Text scoreText;
    private int _score = 0;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void UpdateScoreString()
    {
        //Set score text to score with 8 zero padding
        scoreText.text = _score.ToString("D8");
    }

    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            UpdateScoreString();
        }
    }
}
