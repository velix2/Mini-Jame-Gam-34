using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private GameObject followTarget;
    [SerializeField] private float offset;
    
    
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        var targetPos = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
        
        transform.position = Vector3.Lerp(transform.position, targetPos, offset * Time.deltaTime);
    }
}
