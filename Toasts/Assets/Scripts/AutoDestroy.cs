using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float autoDestroyTime;
    private float autoDestroyCounter;
    void Update()
    {
        autoDestroyCounter += Time.deltaTime;

        if (autoDestroyCounter >= autoDestroyTime)
            Destroy(gameObject);
    }
}
