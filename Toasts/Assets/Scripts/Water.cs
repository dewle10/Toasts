using UnityEngine;

public class Water : MonoBehaviour
{
    private WormHealth wormHealth;
    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            wormHealth = coll.GetComponent<WormHealth>();
            wormHealth.WaterKill();
        }
    }
}
