using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField]
    private float inputTimer, attack1Radius, attack1Damage;
    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private Transform attack1HitBoxPos;
    [SerializeField]
    private LayerMask whatIsDamageable;

    private PlayerController PC;

    private bool gotInput, isAttacking, isFirstAttack;
    //make you able to perform an attack from the start of the game
    private float lastInputTime = Mathf.NegativeInfinity;

    private Animator animator;
    private float resetMovementSpeed = 8.0f;
    private float playerSpeedWhileAttacking = 0.3f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("canAttack", combatEnabled);
        PC = GetComponent<PlayerController>();
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetButton("Attack"))
        {
            if (combatEnabled)
            {
                //attempt Attack
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            //Start attack 1

            if (!isAttacking && !PC.isWallSliding)
            {
                
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                animator.SetBool("attack1", true);
                animator.SetBool("firstAttack", isFirstAttack);
                animator.SetBool("isAttacking", isAttacking);
                if(PC.isGrounded)
                {
                    PC.movementSpeed = playerSpeedWhileAttacking;
                }

            }
        }

        if(Time.time >= lastInputTime + inputTimer)
        {
            // wait for new input
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        foreach(Collider2D collider in detectedObjects)
        {
            //SendMessage is used to call a specific function on a script on object without knowing which script it is
            //so we can have different scripts with different ennemy types and others than than we want to damage
            //and all we need to do is to make sure that they all have a function called "the same thing"
            //in this case, it's "Damage"
            //So we call the damage function and we pass the amout of damage
            collider.transform.parent.SendMessage("Damage", attack1Damage);
            
            //Instantiate hit particle
        }
     }

    private void FinishAttack1()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("attack1", false);
        PC.movementSpeed = resetMovementSpeed;

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
