using System;
using System.Collections;
using System.Collections.Generic;
using SpellFramework.Event;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    public float moveSpeed;

    private Vector2 moveInput;

    private Animator ani;

    private SpriteRenderer sr;

    private DetectionZone dz;

    private bool isFocusNPC = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        dz = GetComponent<DetectionZone>();
    }

    void Start()
    {
    }

    private void FixedUpdate()
    {
        rb.AddForce(moveInput * moveSpeed);
    }

    private void LateUpdate()
    {
        if (dz.DetectCollider != null && isFocusNPC == false)
        {            
            if (dz.DetectCollider.transform.position.x > transform.position.x)
            {
                sr.flipX = false;
                gameObject.BroadcastMessage("IsFaceRight", true);
            }
            else
            {
                sr.flipX = true;
                gameObject.BroadcastMessage("IsFaceRight", false);
            }

            isFocusNPC = true;
            var id = dz.DetectCollider.gameObject.GetComponent<NPCInfo>().DiglogID;
            EventSystem.Dispatch("OnDialogEnter", id);
        }
        else if (dz.DetectCollider == null && isFocusNPC == true) 
        {
            isFocusNPC = false;
            EventSystem.Dispatch("OnDialogExit");
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        
        ani.SetBool("isWalking", moveInput != Vector2.zero);
        if (moveInput.x > 0)
        {
            sr.flipX = false;
            gameObject.BroadcastMessage("IsFaceRight", true);
        }
        else if (moveInput.x < 0)
        {
            sr.flipX = true;
            gameObject.BroadcastMessage("IsFaceRight", false);
        }
    }

    public void OnFire()
    {
        ani.SetTrigger("swordAttack");
    }

    public void OnAttack()
    {
        
    }

    public void OnDead()
    {
        ani.SetTrigger("isDead");
    }
    
    public void OnDamage()
    {
        ani.SetTrigger("isDamage");
    }
    
    public void OnDeadOver()
    {
        Debug.Log("died ....");
        GameObject.Destroy(this.gameObject);
    }

}
