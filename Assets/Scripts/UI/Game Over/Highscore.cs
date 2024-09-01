using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Highscore : MonoBehaviour
{
    private TMP_Text _text;
    private int _storedHighscore;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        FetchAndDisplayHighscore();
    }

    private void FetchAndDisplayHighscore()
    {
        _storedHighscore = PlayerPrefs.GetInt("Highscore", 0);
        if (_storedHighscore < ScoreHandler.Instance.Score)
        {
            //new highscore
            PlayerPrefs.SetInt("Highscore", ScoreHandler.Instance.Score);

            _text.text = "NEW HIGHSCORE!!!";
            _text.color = Color.yellow;
        }
        else
        {
            _text.text = "HIGHSCORE: " + _storedHighscore.ToString("D8");
        }
    }
    
}
