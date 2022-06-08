using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyController : EnemyController
{
    public float Range = 6;
    public float LOSRayRadius = 1f;
    public LayerMask ArrowHitMask;
    public GameObject Arrow;
    public AudioClip BowDrawSFX;

    private Vector3 shootTarget = Vector3.zero;
    private Vector3 shootOrigin = Vector3.zero;
    private bool inRange = false;

    // Override Path find
    protected override void UpdatePath()
    {
        inRange = IsInRange();
        if(isAlive && seeker.IsDone()) {
            if(!inRange) {
               seeker.StartPath(rb.position, Target.position+Target.up*.2f, OnPathComplete); 
            }
            else {
                path = null;
            }
        }
    }
    protected bool IsInRange()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, LOSRayRadius, ((Target.position+Target.up*.5f) - transform.position).normalized, Range, ArrowHitMask);
        return hit.collider != null && hit.collider.CompareTag("PlayerHitbox");
    }

    // Override SubUpdate
    protected override void SubUpdate()
    {
        if(inRange && !isAttacking) {
            Attack(Target.position+Target.up*.5f);
        }
    }

    public void Attack(Vector3 target) {
        isAttacking = true;
        shootTarget = target;
        animator.speed = 1f;
        speedAdjustment = .5f;
        SetAttackPosition();
        animator.SetBool("IsAttacking", true);
        AudioSource.PlayOneShot(BowDrawSFX, .05f);
    }

    public void SetAttackPosition()
    {
        Vector3 diff = shootTarget - (transform.position+Vector3.up*.5f);
        facing.x = diff.x;
        facing.y = diff.y;

        shootOrigin = transform.position;
        if(Mathf.Abs(facing.x) > Mathf.Abs(facing.y)) {
            if(facing.x > 0) {
                shootOrigin += Vector3.up * 0.5f + Vector3.right * 0.4f;
            }
            else {
                shootOrigin += Vector3.up * 0.5f + Vector3.left * 0.4f;
            }
        }
        else {
            if(facing.y > 0) {
                shootOrigin += Vector3.up * 0.75f;
            }
            else {
                shootOrigin += Vector3.up * 0.1f;
            }
        }
        UpdateFacingAnimation();
    }


    public void OnAttackRelease()
    {
        SetAttackPosition();
        Vector3 diff = shootTarget - shootOrigin;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90;
        GameObject arrow = Object.Instantiate(Arrow, shootOrigin, Quaternion.identity);
        Physics2D.IgnoreCollision(arrow.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        arrow.layer = gameObject.layer;
        arrow.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(gameObject.layer);
        arrow.GetComponent<Arrow>().Damage = Damage;
        arrow.GetComponent<Arrow>().IsEnemy = true;
    }

    public void OnAttackFinish() 
    {
        DoneAttacking();
    }
}

