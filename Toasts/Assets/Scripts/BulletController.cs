using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public enum WeaponType { Rocket, grenade, sniper, punch };

    [SerializeField]
    private WeaponType weaponType;

    [SerializeField]
    private bool updateAngle = true;

    public float granadeTime = 3f;

    public Transform bulletSpriteTransform;
    private Rigidbody2D rb;

    public Transform explosionPosition;

    public GameObject playerExplosion;
    public GameObject groundExplosion;
    [SerializeField]
    private GameObject explosionVFX; //particle system

    private Ground ground;
    private WormMenager wormMenager; 
    private AudioMenager audioMenager; 

    private bool hit;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();   
    }
    private void Start()
    {
        wormMenager = GameObject.FindObjectOfType<WormMenager>();
        audioMenager = GameObject.FindObjectOfType<AudioMenager>();
    }
    void Update()
    {
        if (updateAngle)
        {
            Vector2 dir = new Vector2(rb.velocity.x, rb.velocity.y);
            dir.Normalize();
            float angle = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
            if (dir.x < 0f)
            {
                angle = 180 - angle;
            }
            bulletSpriteTransform.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (weaponType == WeaponType.Rocket || weaponType == WeaponType.sniper || weaponType == WeaponType.punch)
        {
            if (!hit && coll.CompareTag("Ground") || coll.CompareTag("Player"))
            {
                hit = true;

                MakeExplosion();

                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (weaponType == WeaponType.grenade)
        {
            if (!hit && collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player"))
            {
                hit = true;
                StartCoroutine(GranadeExplosion());
            }
        }
    }

    private void MakeExplosion()
    {
        audioMenager.PlaySoundExplosion();

        GameObject explosion = Instantiate(explosionVFX);
        explosion.transform.position = explosionPosition.position;

        GameObject groundExpl = Instantiate(groundExplosion);
        groundExpl.transform.position = explosionPosition.position;

        GameObject playerExpl = Instantiate(playerExplosion);
        playerExpl.transform.position = explosionPosition.position;

        DoNextWorm();
    }

    private void DoNextWorm()
    {
        wormMenager.NextWorm();
        wormMenager.EndNextWormCounter();
    }

    public IEnumerator GranadeExplosion()
    {
        yield return new WaitForSeconds(granadeTime);

        MakeExplosion();

        Destroy(gameObject);
    }
}
