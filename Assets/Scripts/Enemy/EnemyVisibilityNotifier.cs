using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibilityNotifier : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    private void OnBecameVisible()
    {
        Debug.Log("Enemy became visible");
        IsVisible = true;
    }
    
    private void OnBecameInvisible()
    {
        Debug.Log("Enemy became invisible");
        IsVisible = false;   
    }
}
