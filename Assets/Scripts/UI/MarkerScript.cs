using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerScript : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform rotationTarget;
    
    private Camera _camera1;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _camera1 = Camera.main;
    }

    private void Update()
    {
        var coords = _camera1.WorldToScreenPoint(target.position);
        var clamped = new Vector2(Mathf.Clamp(coords.x, 0.08f * Screen.width, 0.92f * Screen.width), 
            Mathf.Clamp(coords.y, 0.1f * Screen.height, 0.9f* Screen.height) );
        _rectTransform.anchoredPosition = clamped;
        rotationTarget.rotation = Quaternion.LookRotation(Vector3.forward, coords - (Vector3) clamped);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}