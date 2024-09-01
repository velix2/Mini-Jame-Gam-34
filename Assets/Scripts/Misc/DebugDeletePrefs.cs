using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDeletePrefs : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();
    }
}
