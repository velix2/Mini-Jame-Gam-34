using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void OnClick()
    {
        GameHandler.Instance = null;
        GameAreaHandler.Instance = null;
        FieldHandler.Instance = null;
        ScoreHandler.Instance = null;
        
        SceneManager.LoadScene("Stage");
    }
}
