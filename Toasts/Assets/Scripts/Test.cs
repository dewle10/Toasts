using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private PolygonCollider2D polygonCollider; 

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            polygonCollider.TryUpdateShapeToAttachedSprite();
        }
    }
}
 