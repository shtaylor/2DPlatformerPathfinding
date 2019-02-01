using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeStrike : MonoBehaviour
{
    public float timeBeforeAttackDisappears = .1f;
    float timer = -1;
    public float damage = 20;
    // Degrees.
    public float angleOfAttack;

    private void Awake()
    {
        timer = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > timeBeforeAttackDisappears)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            health.TakeDamage(damage,angleOfAttack);
        }

        if (other.CompareTag("Ragdoll"))
        {
            DixieRagdoll ragdoll = other.GetComponent<DixieRagdoll>();
            ragdoll.SetForceDirection(damage, angleOfAttack);
        }
    }
}