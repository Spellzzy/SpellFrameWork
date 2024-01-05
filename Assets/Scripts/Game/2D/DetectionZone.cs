using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    public float viewRadius;
    public LayerMask layerMask;

    private Collider2D _detectCollider;
    public Collider2D DetectCollider
    {
        get { return _detectCollider; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, viewRadius, layerMask);
        if (col != null)
        {
            _detectCollider = col;
        }else    
        {
            _detectCollider = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, viewRadius);
    }
}
