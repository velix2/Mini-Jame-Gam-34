using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject followTarget;
    [SerializeField] private float offset;

    [SerializeField] private Vector3 bottomLeftCorner;
    [SerializeField] private Vector3 topRightCorner;
    
    
    void Update()
    {
        
        var targetPos = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
        
        var resultInterpolated = Vector3.Lerp(transform.position, targetPos, offset * Time.deltaTime);
        
        var clampedPosition = new Vector3(
            Mathf.Clamp(resultInterpolated.x, bottomLeftCorner.x, topRightCorner.x),
            Mathf.Clamp(resultInterpolated.y, bottomLeftCorner.y, topRightCorner.y),
            resultInterpolated.z
        );
        
        transform.position = clampedPosition;
    }
}
