using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// For the specific characters, Dixie, this is an addition to
/// their Grounded Entity class. It just generates a ragdoll when they are
/// killed.
/// </summary>
public class DixieController : GroundedEntity
{
    public GameObject DixieRagdollPrefab;

    // maybe remove
    //public GroundedEntityAI_PathOptimization pathOptimization;
    //

    protected override void Awake()
    {
        base.Awake();
        //pathOptimization = GameObject.FindGameObjectWithTag("Graph").GetComponent<GroundedEntityAI_PathOptimization>();
    }

    public override void Die(float attackDamage, float angleOfAttack)
    {
        // Need to make ragdoll fly away in direction of hit
        DixieRagdoll ragdoll = Instantiate<GameObject>(DixieRagdollPrefab, transform.position, Quaternion.identity).GetComponent<DixieRagdoll>();
        ragdoll.GetComponent<SpriteRenderer>().flipX = GetComponentInChildren<SpriteRenderer>().flipX;
        ragdoll.SetForceDirection(attackDamage, angleOfAttack);
        DixieSpawner.numberOfDixies--;

        


        Destroy(gameObject);

    }
}