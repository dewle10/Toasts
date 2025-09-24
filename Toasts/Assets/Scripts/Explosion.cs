using UnityEngine;

public class Explosion : MonoBehaviour
{
    public int damage = -10;

    public CircleCollider2D explosionRange;
    public float explosionStrength = 1f;

    private Ground ground;
    private WormHealth wormHealth;
    private Worm worm;

    public bool isGroundExplosion;
    public bool isPlayerExplosion;


    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (isGroundExplosion)
        {
            if (coll.CompareTag("Ground"))
            {
                ground = coll.GetComponent<Ground>();
                ground.MakeHole(explosionRange);
                Destroy(gameObject);
            }
        }

        if (isPlayerExplosion)
        {
            if (coll.CompareTag("Player"))
            {
                worm = coll.GetComponent<Worm>();
                wormHealth = coll.GetComponent<WormHealth>();
                Vector2 explosionPoint = coll.ClosestPoint(transform.position);
                wormHealth.ChangeHealth(transform, damage, explosionRange.bounds.size.x, explosionPoint);
                worm.Knockback(transform, explosionStrength);

                Destroy(gameObject);
            }
        }
    }
}