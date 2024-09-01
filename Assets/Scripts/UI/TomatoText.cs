using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TomatoText : MonoBehaviour
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

        _text.text = GameHandler.Instance.TomatoesCollected.ToString("D2") + "/" + GameHandler.Instance.TomatoesToGameOver.ToString("D2");

    }
}
