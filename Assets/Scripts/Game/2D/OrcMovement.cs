using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator ani;
    private SpriteRenderer sr;
    private DetectionZone dz;
    public float moveSpeed;

    public int attackDamage;
    public float knockForce;

    private bool isWalkingFlag = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        dz = GetComponent<DetectionZone>();

        isWalkingFlag = false;
    }

    public void OnWalk()
    {
        if (isWalkingFlag != false) return;
        isWalkingFlag = true;
        ani.SetBool("isWalking", true);
    }

    public void OnWalkStop()
    {
        if (isWalkingFlag != true) return;
        isWalkingFlag = false;
        ani.SetBool("isWalking", false);
    }

    public void OnDamage()
    {
        isWalkingFlag = false;
        ani.SetTrigger("isDamage");
    }
    
    public void OnDead()
    {
        ani.SetTrigger("isDead");
    }

    public void OnAttack()
    {
        ani.SetTrigger("isAttack");
    }

    public void OnDeadOver()
    {
        GameObject.Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var collider = col.collider;
        IDamageable damageable = collider.GetComponent<IDamageable>();
        if (damageable != null && collider.tag == "Player")
        {
            OnAttack();
            Vector2 dir = collider.transform.position - transform.position;
            Vector2 force = dir * knockForce;
            damageable.OnHit(attackDamage, force);
        }
    }

    void FixedUpdate()
    {
        if (dz.DetectCollider != null)
        {
            Vector2 dir = dz.DetectCollider.transform.position - transform.position;
            if (dir.sqrMagnitude <= dz.viewRadius * dz.viewRadius)
            {
                rb.AddForce(dir.normalized * moveSpeed);
                if (dir.x > 0)
                {
                    sr.flipX = false;
                }else if (dir.x < 0)
                {
                    sr.flipX = true;
                }
                OnWalk();
            }
            else
            {
                OnWalkStop();
            }
        }
    }
}
