using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreGO : MonoBehaviour
{
    private TMP_Text _text;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {

        _text.text = "SCORE: " + ScoreHandler.Instance.Score.ToString("D8");

    }
}
