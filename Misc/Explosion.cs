using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    float radius;
    public float timeUntilDespawnCircleCollider;
    public float timeBeforeDespawn;
    public float timer;
    CircleCollider2D circle;
    Bounds circleBounds;

    public float power = 35;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleBounds = spriteRenderer.bounds;
        radius = circleBounds.extents.x;
        timer = 0;
        circle = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        //if (circle != null)
        //{
        //    circle.radius = circleBounds.extents.x;
        //}

        if (timer > timeUntilDespawnCircleCollider)
        {
            Destroy(circle);
        }

        if (timer > timeBeforeDespawn)
        {
            Destroy(gameObject);
        }

        timer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player") || other.CompareTag("Ragdoll"))
        {
            float angle = Vector2Extension.DirectionAngle(other.transform.position - transform.position);

            if (angle > 180 && angle < 270)
            {
                float amountOver = angle - 180;
                angle -= 2 * amountOver;
            }
            else if (angle >= 270)
            {
                float amountUnder360 = 360 - angle;
                angle = amountUnder360;
            }

            if (other.CompareTag("Ragdoll"))
            {
                DixieRagdoll ragdoll = other.GetComponent<DixieRagdoll>();
                if (ragdoll != null)
                {
                    ragdoll.SetForceDirection(power, angle);
                }
            }
            else
            {
                HealthSystem healthSystem = other.GetComponent<HealthSystem>();


                healthSystem.TakeDamage(25, angle);
            }
            
        }
    }
}
