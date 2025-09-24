using TMPro;
using UnityEngine;

public class WormHealth : MonoBehaviour
{
    [SerializeField]
    private Worm worm;
    private WormMenager wormMenager;
    private AudioMenager audioMenager;
    private Animator animator;
    private int health;
    public int maxHealth = 100;

    [SerializeField]
    private TMP_Text healthTxt;

    void Awake()
    {
        health = maxHealth;
        healthTxt.text = health.ToString();
    }

    private void Start()
    {
        wormMenager = GameObject.FindObjectOfType<WormMenager>();
        audioMenager = GameObject.FindObjectOfType<AudioMenager>();
        animator = GetComponent<Animator>();
    }

    public void ChangeHealth(Transform explPosition, float dmgMax, float explosionRange, Vector2 explosionPoint)
    {
        float distance = Vector2.Distance(explPosition.position, explosionPoint);
        float distanceMultiplier = 1f - (distance / (explosionRange / 2) );
        float finaleDmg = Mathf.Clamp(dmgMax * distanceMultiplier, dmgMax, -1f);

        health += Mathf.FloorToInt(finaleDmg);

        if (health > maxHealth)
            health = maxHealth;
        else if (health <= 0) 
            health = 0;

        healthTxt.text = health.ToString();
        wormMenager.UpdateTeamHP();

        animator.SetTrigger("damage");

        if (health <= 0)
        {
            KillWorm();
        }
        audioMenager.PlaySoundDamage();

    }
    public void WaterKill()
    {
        health += -2;

        if (health > maxHealth)
            health = maxHealth;
        else if (health <= 0)
            health = 0;

        healthTxt.text = health.ToString();
        wormMenager.UpdateTeamHP();

        if (health <= 0)
        {
            KillWorm();
        }
    }

    public int GetWormHP()
    {
        return health;
    }

    public void KillWorm()
    {
        gameObject.SetActive(false);
        wormMenager.RemoveWorm(worm.GetID());
        if (!wormMenager.IswormWating() && worm.IsTurn)
        {
            wormMenager.NextWorm();
        }
    }
}
