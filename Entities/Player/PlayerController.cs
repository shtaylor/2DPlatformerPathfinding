using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : GroundedEntity
{
    


    public GameObject meleeStrikePrefab;
    public MeleeStrike meleeStrike;

    float attackTimer = -1;
    public float timeBeforeAttackReset = .25f;

    Vector2 characterDimensions;
    Vector2 weaponDimensions;

    float halfSquareRoot2;

    public float meleeDamage = 10;

    NavGraphPlatformer graph;

    int attackTriggerHash = Animator.StringToHash("Attack");

    public GameObject bloodPrefab;
    public float bloodParticlesPerSecond = 20;

    protected override void Awake()
    {
        base.Awake();
        characterDimensions = GetComponent<Collider2D>().bounds.size;
        weaponDimensions = meleeStrikePrefab.GetComponent<Collider2D>().bounds.size;

        halfSquareRoot2 = Mathf.Sqrt(2) / 2;

        graph = GameObject.FindGameObjectWithTag("Graph").GetComponent<NavGraphPlatformer>();
        graph.player = this;

        CameraFollow mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
        if (mainCam != null)
        {
            mainCam.target = GetComponent<Controller2D>();
        }
    }

    /// <summary>
    /// Melee Attacks are determined by the combination of directional inputs being held
    /// or in the case that the player is not holding a directional input, then
    /// it's a standard attack in the direction the character is facing.
    /// 
    /// Also, the character must not be on the ground in order to attack down, downleft, or downright
    /// </summary>
    public void MeleeAttack()
    {
        if (attackTimer != -1)
        {
            return;
        }
        bool isGrounded = IsTouchingFloor();

        // We'll start off at the character's position
        Vector3 location = transform.position;
        location.y += characterDimensions.y / 2;
        Quaternion rotation = Quaternion.identity;

        float angleOfAttack = 0;

        bool flipWeaponSprite = false;

        // Attack right
        if ((directionalInput == new Vector2(1,0)) 
            || (directionalInput == Vector2.zero && facingRight) 
            || (directionalInput.y == -1 && directionalInput.x == 0 && isGrounded && facingRight))
        {
            // Instantiate attack to the right of the character.
            // It'll need to be at position + 1/2 the character bounds + 1/2 the sprite box collider
            location.x += 1.2f * characterDimensions.x + weaponDimensions.x;
            //rotation = Quaternion.identity;
        }
        else if ((directionalInput == new Vector2(-1, 0))
            || (directionalInput == Vector2.zero && !facingRight)
            || (directionalInput.y == -1 && directionalInput.x == 0 && isGrounded && !facingRight))
        {
            // Instantiate to the left of the character
            location.x += -1.2f * characterDimensions.x - weaponDimensions.x;
            //rotation = Quaternion.identity;
            flipWeaponSprite = true;

            angleOfAttack = 180;
        }
        else if (directionalInput == new Vector2(0,1))
        {
            // Instantiate straight above the player
            location.y += 1.2f * characterDimensions.y + weaponDimensions.x;
            rotation = Quaternion.Euler(0, 0, 90);

            angleOfAttack = 90;
        }
        else if (directionalInput == new Vector2(0,-1) && !isGrounded)
        {
            // Instantiate directly beneath the player
            location.y += -1.2f * characterDimensions.y - weaponDimensions.x;
            rotation = Quaternion.Euler(0, 0, -90);

            angleOfAttack = 270;
        }
        else
        {
            // These all are going to be diagonally positioned
            Vector3 offset = new Vector3((1.2f * characterDimensions.x + weaponDimensions.x)*halfSquareRoot2, (characterDimensions.y + weaponDimensions.x)*halfSquareRoot2, 0);

            // Diagonal up.
            if (directionalInput.y == 1)
            {

                

                // Right
                if (directionalInput.x == 1)
                {
                    location += offset;
                    rotation = Quaternion.Euler(0, 0, 45);
                    angleOfAttack = 45;
                }
                else
                {
                    offset.x = -offset.x;
                    location += offset;
                    flipWeaponSprite = true;
                    rotation = Quaternion.Euler(0, 0, -45);

                    angleOfAttack = 135;
                }
            }
            // Diagonal down.
            else if(directionalInput.y == -1)
            {


                

                offset.y = -offset.y;
                // Down Right
                if (directionalInput.x == 1)
                {
                    location += offset;
                    rotation = Quaternion.Euler(0, 0, -45);

                    angleOfAttack = 315;
                }
                else
                {
                    offset.x = -offset.x;
                    location += offset;
                    flipWeaponSprite = true;
                    rotation = Quaternion.Euler(0, 0, 45);
                    angleOfAttack = 225;
                }
            }
            else
            {
                Debug.Log("Somehow the directional input & state didn't lead to one of the predicted weapon spawn locations");
            }
        }
        GameObject meleeGameObject = Instantiate<GameObject>(meleeStrikePrefab, location, rotation,transform);
        meleeGameObject.GetComponent<SpriteRenderer>().flipX = flipWeaponSprite;
        meleeGameObject.transform.localScale = meleeGameObject.transform.localScale / transform.localScale.x;
        meleeStrike = meleeGameObject.GetComponent<MeleeStrike>();
        meleeStrike.angleOfAttack = angleOfAttack;
        meleeStrike.damage = meleeDamage;
        attackTimer = 0;

        TriggerAttackAnim();
    }

    protected override void Update()
    {

        base.Update();
        
        if (attackTimer >= 0)
        {
            attackTimer += Time.deltaTime;
        }
        if (attackTimer >= timeBeforeAttackReset)
        {
            attackTimer = -1;
        }

        
    }

    protected virtual void TriggerAttackAnim()
    {
        anim.SetTrigger(attackTriggerHash);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // This prevents some cases of enemies jumping off ledges to their deaths
        if (other.CompareTag("Node"))
        {
            Node node = other.GetComponent<Node>();
            if (node.nodeType == GameNodeTypes.FloorNode)
            {
                graph.playerLastNodeID = node.ID;
            }
        }

        if (other.CompareTag("NPC") && !isInvulnerable && !isDead)
        {
            if (other.transform.position.x < transform.position.x)
            {
                // knock back to the right.
                GetKnockedBack(true);
            }
            else
            {
                GetKnockedBack(false);
            }
            //Die(0);
            //StartCoroutine(SpawnBlood());
        }
    }
}