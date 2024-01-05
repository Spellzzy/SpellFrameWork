using System;
using UnityEngine;

public class DamageCharacter : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    private Collider2D col;

    public int health;

    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (health <= 0)
            {
                Targetable = false;
                gameObject.BroadcastMessage("OnDead");
            }
            else
            {
                gameObject.BroadcastMessage("OnDamage");
            }
        }
    }

    private bool targetable = true;

    public bool Targetable
    {
        get { return targetable; }
        set
        {
            targetable = value;
            if (!targetable)
            {
                rb.simulated = false;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void OnHit(int damage, Vector2 knockBack)
    {
        Health = Health - damage;
        rb.AddForce(knockBack);
    }
}