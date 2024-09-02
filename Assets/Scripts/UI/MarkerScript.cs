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

    //TODO. Improve this
    private void Update()
    {
        var coords = _camera1.WorldToViewportPoint(target.position);
        float scale = 1920f / Screen.width;
        coords = new Vector3(coords.x * Screen.width, coords.y * Screen.height, 0) * scale;
        var clamped = new Vector2(Mathf.Clamp(coords.x, 0.08f * Screen.width * scale, 0.92f * Screen.width * scale), 
            Mathf.Clamp(coords.y, 0.1f * Screen.height * scale, 0.9f* Screen.height * scale) );
        _rectTransform.anchoredPosition = clamped;
        rotationTarget.rotation = Quaternion.LookRotation(Vector3.forward, coords - (Vector3) clamped);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}