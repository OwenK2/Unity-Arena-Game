using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDamageRange : MonoBehaviour
{
    EnemyController ec;
    // Start is called before the first frame update
    void Start()
    {
        ec = GetComponentInParent<EnemyController>();
    }

    void OnTriggerStay2D(Collider2D other) 
    {
        if(other.CompareTag("Player")) {
            if(ec.CanDamage()) {
                ec.DoDamage();
                other.GetComponent<Player>().OnDamage(ec.Damage);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other) 
    {
        if(other.CompareTag("Player")) {
            ec.DoneAttacking();
        }
    }
}
