using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProgressBarInWorld : MonoBehaviour
{
    [SerializeField] private Sprite[] progressSprites;
    [SerializeField] private Color colorFull;
    [SerializeField] private Color colorEmpty;
    [SerializeField] private float maxValue;
    
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void SetMaxValue(float value)
    {
        maxValue = value;
    }
    
    public void SetValue(float value)
    {
        if (value < 0) value = 0;
        if (value > maxValue) value = maxValue;
        
        var progress = value / maxValue;
        var spriteIndex = Mathf.FloorToInt(progress * (progressSprites.Length - 1));
        _spriteRenderer.sprite = progressSprites[spriteIndex];
        _spriteRenderer.color = Color.Lerp(colorEmpty, colorFull, progress);
    }
    
    public void SetVisible(bool visible)
    {
        _spriteRenderer.enabled = visible;
    }
    
}
