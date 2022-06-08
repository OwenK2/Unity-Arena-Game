using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float Damage = 1f;
    public float Force = 15f;
    public float Lifespan = 5f;
    public float LingerTime = 1f;
    public bool IsSpecial = false;
    public bool IsEnemy = false;
    public Sprite ArrowSprite;
    public Sprite SpecialArrowSprite;
    public Sprite HitSprite;
    public Sprite HitSpecialSprite;
    public List<AudioClip> HitSounds = new List<AudioClip>();
    private AudioSource AudioSource;
    private HashSet<GameObject> pastTargets = new HashSet<GameObject>();
    private bool isEmbedded = false;
    private GameObject embeddedTarget = null;
    private Vector3 embeddedOffset = Vector3.zero;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        AudioSource = GetComponent<AudioSource>();
        ps = GetComponent<ParticleSystem>();
        psRenderer = ps.GetComponent<ParticleSystemRenderer>();

        rb.AddForce(transform.up * Force, ForceMode2D.Impulse);
        AudioSource.PlayOneShot(AudioSource.clip, .5f);
        Invoke(nameof(Remove), Lifespan);
        
        psRenderer.sortingLayerID = sr.sortingLayerID;
        psRenderer.sortingOrder = sr.sortingOrder;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys( new GradientColorKey[] { new GradientColorKey(GameController.Instance.PlayerColor, 0.0f), new GradientColorKey(Color.black, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } );
        col.color = grad;
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseMenuUI.IsPaused) {return;}
        if(isEmbedded) {
            if(embeddedTarget != null) {
                transform.position = embeddedTarget.transform.position + embeddedOffset;
            }
            else {Remove();}
        }
    }


    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if(isEmbedded) {return;}

        if(IsEnemy) { //Enemies arrow
            if(!hitInfo.isTrigger && !hitInfo.CompareTag("Player") && !hitInfo.CompareTag("Enemy")) {HitTarget(hitInfo.gameObject);}
            else if(hitInfo.CompareTag("PlayerHitbox")) {
                Player player = hitInfo.gameObject.GetComponentInParent<Player>();
                if(player != null) {
                    player.OnDamage(Damage);
                    HitTarget(hitInfo.gameObject);
                } 
            }
        }
        else { // Player arrow
            if(!hitInfo.isTrigger && !hitInfo.CompareTag("Enemy")) {HitTarget(hitInfo.gameObject);}
            else if(hitInfo.CompareTag("EnemyHitbox") && (!IsSpecial || !pastTargets.Contains(hitInfo.gameObject))) {
                EnemyController ec = hitInfo.gameObject.GetComponentInParent<EnemyController>();
                if(ec != null) {
                    ec.TakeDamage(Damage * (IsSpecial ? 2f : 1f));
                    if(IsSpecial) {
                        pastTargets.Add(hitInfo.gameObject); 
                    }
                    else {HitTarget(hitInfo.gameObject);} 
                }
            }
        }
    }

    void HitTarget(GameObject target)
    {
        embeddedTarget = target;
        embeddedOffset = transform.position - target.transform.position;
        isEmbedded = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        DisableColliders();
        sr.sprite = IsSpecial ? HitSpecialSprite : HitSprite;
        if(HitSounds.Count > 0) {
            AudioSource.PlayOneShot(HitSounds[Random.Range(0, HitSounds.Count)], .1f);
        }
        Invoke(nameof(Remove), LingerTime);
    }

    void Remove()
    {
        Destroy(gameObject);
    }

    void DisableColliders()
    {
        Collider2D[] colliders = new Collider2D[rb.attachedColliderCount];
        int numColliders = rb.GetAttachedColliders(colliders);
        for(int i = 0; i < numColliders; ++i) {
            colliders[i].enabled = false;
        }
    }

}
