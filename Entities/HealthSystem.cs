using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float health = 1;

    public GroundedEntity ent;

    // Degrees
    float lastAngleOfAttack = 0;
    float lastDamageOfAttack = 0;

    private void Awake()
    {
        ent = GetComponent<GroundedEntity>();
    }

    private void Update()
    {
        if (health <= 0 && !ent.isDead)
        {
            health = 0;
            ent.Die(lastDamageOfAttack, lastAngleOfAttack);
        }
    }

    public void TakeDamage(float damage, float angleOfAttack)
    {
        lastDamageOfAttack = damage;
        lastAngleOfAttack = angleOfAttack;
        health -= damage;
    }
}
