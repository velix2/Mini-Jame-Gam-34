using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveText : MonoBehaviour
{
    private TMP_Text _text;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        UpdateText();
    }
    
    public void UpdateText()
    {
        gameObject.SetActive(true);

        _text.text = GameHandler.Instance.IsWaveInProgress ? "Wave " + GameHandler.Instance.CurrentWave
                : "Upcoming: Wave " + (GameHandler.Instance.CurrentWave + 1);

    }
}
