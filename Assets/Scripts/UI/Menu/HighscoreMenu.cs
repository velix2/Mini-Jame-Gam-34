using System;
using TMPro;
using UnityEngine;

public class HighscoreMenu : MonoBehaviour
{
    private void OnEnable()
    {
        var storedHighscore = PlayerPrefs.GetInt("Highscore", 0);
        GetComponent<TMP_Text>().text = "Your Highscore: " + storedHighscore.ToString("D8");
    }
}