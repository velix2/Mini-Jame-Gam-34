using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class RenderSorting : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private float offsetY;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        var y = transform.position.y + offsetY;
        var intY = (int) (y * 100f);
        
        _spriteRenderer.sortingOrder = -intY;
    }
}
