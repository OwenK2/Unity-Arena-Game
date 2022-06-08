using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float MaxHealth = 100f;
    public float Health = 100f;
    public float speed = 2.0f;
    public float Damage = 10f;
    public float Vitality = .5f;
    public float SpecialAttackTimeout = 10f;
    public Color HurtColor = Color.red;
    public float HurtColorTime = .3f;
    public GameObject Arrow;
    public AudioClip BowDrawSFX;
    public AudioClip HurtSFX;
    public AudioClip DieSFX;
    public ArenaController ArenaController = null;
    private float basespeed = 1f;
    private bool isAlive = true;
    private bool isAttacking = false;
    private bool isMoving = false;
    private float speedAdjustment = 1.0f;
    private bool isDamaged = false;
    private float hurtSoundCoolDown = 0;
    private float specialTimeoutCount = 0;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AudioSource AudioSource;
    private Animator animator;
    private Vector2 movement = Vector2.zero;
    private Vector2 facing = Vector2.zero;
    private Vector3 shootTarget = Vector3.zero;
    private Vector3 shootOrigin = Vector3.zero;
    private bool nextAttackSpecial = false;
    private float vitalityCountdown = 1f;

    void Awake()
    {
        basespeed = speed;
        animator = GetComponent<Animator>();
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAlive", isAlive);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetFloat("left", 0f);
        animator.SetFloat("right", 0f);
        animator.SetFloat("up", 0f);
        animator.SetFloat("down", 0f);
        animator.speed = 1f;
    }
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        AudioSource = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {
        if(PauseMenuUI.IsPaused) {return;}

        if(isDamaged) {
            sr.color = Color.Lerp(sr.color, HurtColor, Time.deltaTime * 10f);
        }
        else if(sr.color != Color.white) {
            sr.color = Color.Lerp(sr.color, Color.white, Time.deltaTime * 10f);
        }

        if(!isAlive) {return;}

        if(hurtSoundCoolDown > 0) {hurtSoundCoolDown -= Time.deltaTime;}
        if(specialTimeoutCount > 0) {
            specialTimeoutCount -= Time.deltaTime;
            ArenaController.UpdatePlayerSpecialUI(1f - (specialTimeoutCount / SpecialAttackTimeout));
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        isMoving = movement.sqrMagnitude > 1e-5;
        animator.SetBool("IsMoving", isMoving);

        if(isMoving && !isAttacking) {
            facing.x = movement.x;
            facing.y = movement.y;
            UpdateFacingAnimation();
        }

        if(Input.GetButtonDown("Jump") && specialTimeoutCount <= 0) {
            if(isAttacking) {
                animator.SetBool("CancelAttack", true);
                specialTimeoutCount = SpecialAttackTimeout;
                Invoke(nameof(SpecialAttack), .1f);
            }
            else {SpecialAttack();}
        }
        else if(!isAttacking && Input.GetAxisRaw("Fire1") > 0) {
            Attack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }


        // Vitality 
        vitalityCountdown -= Time.deltaTime;
        while(vitalityCountdown <= 0) {
            vitalityCountdown += 1;
            Health = Mathf.Clamp(Health + Vitality, 0f, MaxHealth);
            ArenaController.UpdatePlayerHealthUI();
        }
    }

    void UpdateFacingAnimation()
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

    void FixedUpdate() 
    {
        rb.MovePosition(rb.position + movement * speed * speedAdjustment * Time.fixedDeltaTime);
    }

    public void SpecialAttack()
    {
        animator.SetBool("CancelAttack", false);
        nextAttackSpecial = true;
        specialTimeoutCount = SpecialAttackTimeout;
        Attack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
    public void Attack(Vector3 target) {
        isAttacking = true;
        shootTarget = target;
        speedAdjustment = 0.5f;
        animator.speed = 1f;
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
        Vector3 diff = shootTarget - (transform.position+Vector3.up*.5f);
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90;
        GameObject arrow = Object.Instantiate(Arrow, shootOrigin, Quaternion.identity);
        Arrow arrowController = arrow.GetComponent<Arrow>();
        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        Physics2D.IgnoreCollision(arrow.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        arrow.layer = gameObject.layer;
        sr.sortingLayerName = LayerMask.LayerToName(gameObject.layer);
        arrowController.Damage = Damage;
        if(nextAttackSpecial) {
            ParticleSystem ps = arrow.GetComponent<ParticleSystem>();
            arrowController.IsSpecial = true;
            sr.sprite = arrowController.SpecialArrowSprite;
            sr.color = GameController.Instance.PlayerColor;
            ps.Play();
            nextAttackSpecial = false;
        }
    }

    public void OnAttackFinish() 
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        animator.speed = speed / basespeed;
        speedAdjustment = 1.0f;
    }

    public void OnDamage(float damage)
    {
        if(!isAlive) {return;}
        CancelInvoke(nameof(UntintSprite));
        Health -= damage;
        if(Health <= 0) {
            //Dead
            isAlive = false;
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            animator.SetBool("IsAlive", isAlive);
            AudioSource.PlayOneShot(DieSFX, .4f);
            UntintSprite();
        }
        else {
            isDamaged = true;
            if(hurtSoundCoolDown <= 0) {
                AudioSource.PlayOneShot(HurtSFX, .2f);
                hurtSoundCoolDown = 1f;
            }
            Invoke(nameof(UntintSprite), HurtColorTime);
        }
        ArenaController.UpdatePlayerHealthUI();
    }
    void UntintSprite()
    {
        isDamaged = false;
    }

    void OnFinishDeathAnimation()
    {
        Invoke(nameof(FinalDeath), 2f);
    }
    void FinalDeath()
    {
        ArenaController.GameOver();
    }

    public void UpgradeHealth()
    {
        MaxHealth += Mathf.Clamp(MaxHealth * 0.2f, 10f, 50f);
        Health = MaxHealth;
        ArenaController.UpdatePlayerHealthUI();
    }
    public void UpgradeDamage()
    {
        Damage += Mathf.Clamp(Damage * 0.15f, 5f, 10f);
        ArenaController.UpdatePlayerDamageUI();
    }
    public void UpgradeSpeed()
    {
        speed += Mathf.Clamp(speed * 0.05f, .01f, .1f);
    }
}
