using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DixieRagdoll : MonoBehaviour
{
    //public float power = 50;
    //public float torquePower = 25;
    Rigidbody2D rb;
    public float timeBeforeDespawn = 2;
    public float timeBeforeStartBlinking = 1.75f;
    
    public float timer;

    public GameObject ExplosionPrefab;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= timeBeforeStartBlinking)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }

        if (timer > timeBeforeDespawn)
        {
            Vector3 ExplosionSpawnPosition = transform.position;
            //ExplosionSpawnPosition.y -= transform.localScale.y / 2;
            Instantiate<GameObject>(ExplosionPrefab, ExplosionSpawnPosition, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void SetForceDirection(float power, float angleOfAttack)
    {

        

        Vector2 force;
        float x;
        float y;

        float torque;

        if (angleOfAttack == 0)
        {
            x = 1;
            y = .35f;
            torque = -power;
        }
        else if (angleOfAttack == 45)
        {
            x = 1;
            y = 1;
            torque = -power;
        }
        else if (angleOfAttack == 90)
        {
            x = -.1f;
            y = 1;
            torque = power;
        }
        else if (angleOfAttack == 135)
        {
            x = -1;
            y = 1;
            torque = power;
        }
        else if (angleOfAttack == 180)
        {
            x = -1;
            y = .35f;
            torque = power;
        }
        else if (angleOfAttack == 225)
        {
            x = -1;
            y = -1;
            torque = -power;
        }
        else if (angleOfAttack == 270)
        {
            x = .25f;
            y = -1;
            torque = -power;
        }
        else if (angleOfAttack == 315)
        {
            x = 1;
            y = -1;
            torque = -power;
        }
        else
        {
            // This is primarily for if the angle is between say 1 and 179
            float radAngle = angleOfAttack * Mathf.Deg2Rad;

            y = Mathf.Abs(Mathf.Tan(radAngle));
            if (angleOfAttack > 90)
            {
                x = -1;
                torque = power;
            }
            else
            {
                x = 1;
                y = Mathf.Tan(radAngle);
                torque = -power;
            }
        }
        force = new Vector2(x, y);
        force = force.normalized * power;

        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }
}
