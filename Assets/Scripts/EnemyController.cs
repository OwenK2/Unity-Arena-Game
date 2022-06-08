using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public Transform Target;
    public float MaxHealth = 50f;
    public float Speed = 1f;
    public float NextWaypointDistanceSquared = .2f;
    public float UpdateInterval = .5f;
    public float Damage = 5f;
    public float DamageTimeout = 1f;
    public float HurtColorTime = .3f;
    public Color MaxHealthColor = Color.green;
    public Color MidHealthColor = Color.yellow;
    public Color MinHealthColor = Color.red;
    public Color HurtColor = Color.red;
    public List<AudioClip> HurtSFX = new List<AudioClip>();
    public List<AudioClip> DieSFX = new List<AudioClip>();
    public ArenaController ArenaController = null;

    protected Path path;
    protected Seeker seeker;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer sr;
    protected AudioSource AudioSource;
    protected RectTransform Healthbar;
    protected Image HealthbarImage;


    protected Vector2 movement = Vector2.zero;
    protected Vector2 facing = Vector2.zero;
    protected bool isAttacking = false;
    protected float basespeed = 1f;
    protected float speedAdjustment = 1f;
    protected float MaxHealthbarWidth = 1f;
    protected float hp;
    protected float hurtSoundCoolDown = 0;
    protected int currentWaypoint = 0;
    protected bool isMoving = false;
    protected bool damagedTint = false;
    protected bool canTakeDamage = false;
    protected bool canDamage = false;
    protected bool canMove = false;
    protected bool isAlive = true;

    void Awake()
    {
        basespeed = Speed;
        animator = GetComponent<Animator>();
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAlive", isAlive);
        animator.SetFloat("left", 0f);
        animator.SetFloat("right", 0f);
        animator.SetFloat("up", 0f);
        animator.SetFloat("down", 0f);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        AudioSource = GetComponent<AudioSource>();
        hp = MaxHealth;

        // Continuously update path
        InvokeRepeating(nameof(UpdatePath), 0f, UpdateInterval);

        Healthbar = transform.Find("HealthbarCanvas/Healthbar").GetComponent<RectTransform>();
        HealthbarImage = Healthbar.GetComponent<Image>();
        MaxHealthbarWidth = Healthbar.sizeDelta.x;
        UpdateHealthbar();
    }

    public void SetStats(float EnemyMaxHealth, float EnemySpeed, float EnemyDamage, float EnemyDamageTimeout)
    {
        MaxHealth = EnemyMaxHealth;
        hp = MaxHealth;
        Speed = EnemySpeed;
        Damage = EnemyDamage;
        DamageTimeout = EnemyDamageTimeout;
        animator.speed = Speed / basespeed;
    }

    protected virtual void UpdatePath()
    {
        if(isAlive && seeker.IsDone()) {
            seeker.StartPath(rb.position, Target.position+Target.up*.2f, OnPathComplete);
        }
    }

    protected void OnPathComplete(Path p)
    {
        if(!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    protected void Update()
    {
        if(PauseMenuUI.IsPaused) {return;}

        if(damagedTint) {
            sr.color = Color.Lerp(sr.color, HurtColor, Time.deltaTime*10f);
        }
        else if(sr.color != Color.white) {
            sr.color = Color.Lerp(sr.color, Color.white, Time.deltaTime*10f);
        }

        if(!isAlive) {return;}


        movement = Vector2.zero;
        if(canMove && path != null) {
            float distance;
            while(true) {
                distance = (rb.position - (Vector2)path.vectorPath[currentWaypoint]).sqrMagnitude;
                if(distance < NextWaypointDistanceSquared && currentWaypoint + 1 < path.vectorPath.Count) {
                    ++currentWaypoint;
                }
                else {
                    break;
                }
            }
            movement = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        }

        // Animation Handling
        isMoving = movement != Vector2.zero;
        animator.SetBool("IsMoving", isMoving);
        if(isMoving && !isAttacking) {
            facing.x = movement.x;
            facing.y = movement.y;
        }
        UpdateFacingAnimation();
        hurtSoundCoolDown -= Time.deltaTime;
        this.SubUpdate();
    }
    protected virtual void SubUpdate() {}

    protected void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * Speed * speedAdjustment * Time.fixedDeltaTime);
    }

    protected void UpdateFacingAnimation()
    {
        bool xBigger = Mathf.Abs(facing.x) > Mathf.Abs(facing.y);
        animator.SetFloat("left", 0f);
        animator.SetFloat("right", 0f);
        animator.SetFloat("up", 0f);
        animator.SetFloat("down", 0f);
        if(xBigger) {
            if(facing.x > 0) {animator.SetFloat("right", 1f);}
            else {animator.SetFloat("left", 1f);}
        }
        else {
            if(facing.y > 0) {animator.SetFloat("up", 1f);}
            else {animator.SetFloat("down", 1f);}  
        } 
    }

    public bool CanDamage()
    {
        return canDamage && isAlive;
    }
    protected void ReallowDamage()
    {
        canDamage = true;
    }
    public void DoDamage()
    {
        animator.speed = 1f;
        isAttacking = true;
        animator.SetBool("IsAttacking", true);
        speedAdjustment = .5f;
        canDamage = false;
        Invoke(nameof(ReallowDamage), DamageTimeout);
    }
    public void DoneAttacking()
    {
        speedAdjustment = 1f;
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        animator.speed = Speed / basespeed;
    }
    public void TakeDamage(float damage)
    {
        CancelInvoke(nameof(UntintSprite));
        if(!canTakeDamage) {return;}
        hp -= damage;
        if(hp <= 0) {
            isAlive = false;
            canTakeDamage = false;
            animator.SetBool("IsAlive", isAlive);
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            AudioSource.PlayOneShot(DieSFX[Random.Range(0, DieSFX.Count)], .5f);
            DisableColliders();
        }
        else {
            damagedTint = true;
            if(hurtSoundCoolDown <= 0) {
                AudioSource.PlayOneShot(HurtSFX[Random.Range(0, HurtSFX.Count)], .5f);
                hurtSoundCoolDown = 1f;
            }
            Invoke(nameof(UntintSprite), HurtColorTime);
        }
        UpdateHealthbar();
    }
    protected void UntintSprite()
    {
        damagedTint = false;
    }
    protected void UpdateHealthbar()
    {
        if(hp < MaxHealth) {
            float percentHP = hp / MaxHealth;
            if(percentHP < .05f && isAlive) {percentHP = .05f;}
            Healthbar.sizeDelta = new Vector2(percentHP * MaxHealthbarWidth, Healthbar.sizeDelta.y);
            if(percentHP > .5f) {
                HealthbarImage.color = Color.Lerp(MidHealthColor, MaxHealthColor, percentHP*2f - 1);
            }
            else {
                HealthbarImage.color = Color.Lerp(MinHealthColor, MidHealthColor, percentHP*2f);
            }
        }
        else {
            HealthbarImage.color = new Color(0f, 0f, 0f, 0f);
        }
    }
    public void FinishDeathAnimation()
    {
        Invoke(nameof(FinalDeath), 1f);
    }
    public void FinalDeath()
    {
        if(ArenaController != null) {
            ArenaController.OnEnemyDeath();
        }
        Destroy(gameObject);
    }
    public void FinishWakeAnimation()
    {
        canMove = true;
        canDamage = true;
        canTakeDamage = true;
    }

    protected void DisableColliders()
    {
        Collider2D[] colliders = new Collider2D[rb.attachedColliderCount];
        int numColliders = rb.GetAttachedColliders(colliders);
        for(int i = 0; i < numColliders; ++i) {
            colliders[i].enabled = false;
        }
    }
}