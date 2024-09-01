using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleControlsDisplay : MonoBehaviour
{
    [SerializeField] private GameObject controlsDisplay;
    [SerializeField] private GameObject infoText;
    private bool _isDisplaying = false;

    private void ToggleDisplay()
    {
        _isDisplaying = !_isDisplaying;
        controlsDisplay.SetActive(_isDisplaying);
        infoText.SetActive(!_isDisplaying);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleDisplay();
        }
    }
}
