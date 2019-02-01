using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// This is the main character controller for both the player and the AI
/// controlled grounded entities. It includes all major movement options
/// including walking, running, jumping, double jumping, wall sliding,
/// wall jumping, dashing, etc...
/// It also handles how animations are chosen depending on the character's
/// state.
/// </summary>
public class GroundedEntity : BaseGameEntity {
    
    // Gravity might not be set right in the BaseGameEntity class.
    public float gravityOverride = -90;

    // Every BaseGameEntity will have a 2D controller
    public Controller2D Controller { get; set; }

    // For a 1x1 square, gravity around -46 feels pretty good.
    // So, if a typical character is somewhere around 5x5, I think that
    // -46*5 should feel nice
    public float Gravity { get; set; }

    public Vector3 velocity;
    // deltaVelocity is communicated to the Controller2D instance
    public Vector2 deltaVelocity;

    public Vector2 directionalInput = Vector2.zero;

    //public int facingDirection = 1;//1 refers to right, 0 to forward, -1 to left
    public bool facingRight = true;

    // Horizontal Velocities
    public float moveSpeed = 15;
    public float walkSpeed = 15;

    // Horizontal Acceleration.
    public float velocityXSmoothing = 0.05f;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    
    // Jumping
    public float jumpVelocity = 33;
    public float airborneTimer = 0;
    public float jumpTimer = 0;
    public float minJumpTime = .1f;
    public bool isHoldingJump = false;

    // Double Jumping
    protected bool canDoubleJump;

    // Fall damage
    float prevFrameVelocityY;
    public float fallDamageVelocityThreshold = -100;
    public float fallDeathVelocityThreshold = -150;


    
    // For animations
    SpriteRenderer spriteRenderer;
    protected Animator anim;
    int walkTriggerHash = Animator.StringToHash("Walk");
    int idleTriggerHash = Animator.StringToHash("Idle");
    int jumpTriggerHash = Animator.StringToHash("Jump");
    int landTriggerHash = Animator.StringToHash("Land");
    int doubleJumpTriggerHash = Animator.StringToHash("DoubleJump");
    int wallSlideTriggerHash = Animator.StringToHash("WallSlide");
    int sprintTriggerHash = Animator.StringToHash("Sprint");
    int dodgeReveresedTriggerHash = Animator.StringToHash("DodgeReversed");
    int dodgeTriggerHash = Animator.StringToHash("Dodge");
    int deathTriggerHash = Animator.StringToHash("Death");
    int resetTriggerHash = Animator.StringToHash("Reset");
    
    // For resetting the character.
    Stack<Vector3> lastGoodPositions;
    Vector3 finalGoodPosition;
    Vector3 initialGoodPosition;


    // Wallsliding.
    public Vector2 wallJumpClimb = new Vector2(17, 30);
    public Vector2 wallJumpOff = new Vector2(12, 20);
    public Vector2 wallLeap = new Vector2(30, 30);
    public float wallSlideSpeedMax = 7;
    public float wallStickTime = 0.25f;
    float timeToWallUnstick = .33f;
    bool isWallSliding;
    int wallDirX;

    // Sprinting
    bool isHoldingSprint = false;
    //Sprinting and maintaining horizontal momentum in air
    public float accelerationTimeGroundedSprinting = 0.2f;
    public float sprintSpeedMultiplier = 2.2f;
    public bool inAirMaintainXVelocity = false;
    float currentHorizontalMoveSpeed = 0;

    // Dodging
    protected bool isDodging = false;
    protected bool isInvulnerable = false;
    bool canDodgeLeft = true;
    bool canDodgeRight = true;
    bool isDodgingRight = false;
    float dodgeTimer = -1;
    public Vector2 dodgeVelocity = new Vector2(40,20);
    public float dodgeDuration = .5f;
    public float invulnerableDuration;

    // Need to make sure to not add things like lava, spike traps, etc... to this list.
    // It should only contain tags such as "SwordSlash", "Bullet", etc... 
    public List<string> TagsThatCanCauseDamage;


    // Death
    public bool isDead = false;
    // These only apply to NPC's with this script
    public float deathTimer = -1;
    public float timeBeforeDespawnAfterDeath = 1;

    // Navigation Graph Stuff.
    public int lastNodeID;

    public bool isBeingKnockedBack = false;
    public bool knockedBackToTheRight;
    public float knockedBackTimer = -1;
    public float knockedBackDuration = .2f;
    public Vector2 knockedBackVelocity = new Vector2(40, 40);


    protected override void Awake()
    {
        base.Awake();
        Controller = GetComponent<Controller2D>();
        Gravity = gravityOverride;
        velocity = Vector3.zero;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        lastGoodPositions = new Stack<Vector3>();

        if (transform.position == null)
        {
            lastGoodPositions.Push(new Vector3(0, 0, 0));
        }
        else
        {
            lastGoodPositions.Push(transform.position);
        }

        finalGoodPosition = lastGoodPositions.Peek();
        initialGoodPosition = lastGoodPositions.Peek();
        Controller.collisions.below = false;

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            ID = "Player";
        }
        else
        {
            ID = this.GetType().ToString() + ":" + transform.position + ":" +  Time.time;
        }

        invulnerableDuration = dodgeDuration / 2;
    }

    protected virtual void Update()
    {

        if (deathTimer >= 0 && CompareTag("NPC"))
        {
            Debug.Log("DEATHTIMER SHOULD BE INCREASING!!!");
            deathTimer += Time.deltaTime;
            if (deathTimer >= timeBeforeDespawnAfterDeath)
            {
                Destroy(gameObject);
            }
        }
        
        CheckConditionsAtStartOfUpdate();

        CalculateAndPerformMovement();

        SetSprite();
    }

    protected virtual void CheckConditionsAtStartOfUpdate()
    {

        

        if (IsTouchingFloor() || IsTouchingRoof())
        {
            if (IsTouchingFloor())
            {
                airborneTimer = 0;
                canDodgeLeft = true;
                canDodgeRight = true;
            }
            
            velocity.y = 0;
            isHoldingJump = false;
            inAirMaintainXVelocity = false;
        }
        else
        {
            airborneTimer += Time.deltaTime;
        }

        if (IsTouchingWall())
        {
            canDodgeLeft = true;
            canDodgeRight = true;
        }
        
    }

    public bool IsTouchingASurface()
    {
        return (Controller.collisions.below || Controller.collisions.above || Controller.collisions.right || Controller.collisions.left);
        
    }

    public bool IsTouchingWall()
    {
        return (Controller.collisions.left || Controller.collisions.right);
    }

    public bool IsTouchingRoof()
    {
        return Controller.collisions.above;
    }

    public bool IsTouchingFloor()
    {
        return Controller.collisions.below;
    }

    public bool IsTouchingJumpableSurface()
    {
        return (IsTouchingFloor() || IsTouchingWall());
    }

    protected virtual void CalculateAndPerformMovement()
    {
        CalculateHorizontalVelocity();


        CalculateVerticalVelocity();

        HandleWallSliding();


        if (isDead)
        {
            currentHorizontalMoveSpeed = 0;
            prevFrameVelocityY = 0;
            velocity = Vector3.zero;
        }

        deltaVelocity = velocity * Time.deltaTime;
        

        // The true argument of Move refers to standing on a platform?
        Controller.Move(deltaVelocity, directionalInput);
        CheckFallDamage();
        prevFrameVelocityY = velocity.y;
    }
    
    protected void CheckFallDamage()
    {
        if ((prevFrameVelocityY < fallDeathVelocityThreshold) && Controller.collisions.below)
        {
            Die(0);
        }
        else if ((prevFrameVelocityY < fallDamageVelocityThreshold) && Controller.collisions.below)
        {
            // Take damage.
        }

    }

    public virtual void Die(float angleOfAttack)
    {
        velocity = Vector3.zero;
        isDead = true;
        TriggerDeathAnim();
        prevFrameVelocityY = 0;
        if (CompareTag("NPC"))
        {
            Debug.Log("NPC HAS DIED!!!");
            deathTimer = 0;
        }
    }

    public virtual void Die(float attackDamage, float angleOfAttack)
    {
        // This gets overridden where it needs to be.
        Die(angleOfAttack);
    }

    public void ResetPosition()
    {

        velocity = Vector3.zero;
        if (lastGoodPositions.Count > 0)
        {
            finalGoodPosition = lastGoodPositions.Peek();
            transform.position = lastGoodPositions.Pop();

        }
        else
        {
            transform.position = initialGoodPosition;
        }

    }

    public void SavePosition()
    {
        lastGoodPositions.Push(transform.position);
    }

    public void ResetGame()
    {
        isDead = false;
        HealthSystem healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.health = 1;
        }
        TriggerResetAnim();
        transform.position = initialGoodPosition;

    }

    protected virtual void CalculateHorizontalVelocity()
    {
        if (isDodging)
        {
            CaclulateDodgeHorizontalMovement();
            currentHorizontalMoveSpeed = velocity.x;
            return;
        }

        if (isBeingKnockedBack)
        {
            CalculateKnockedBackHorizontalMovement();
            currentHorizontalMoveSpeed = velocity.x;
            return;
        }

        float targetVelocityX = directionalInput.x * moveSpeed;


        // Copied from old script:
        // Maintains horizontal velocity when sprinting. Feels pretty nice.
        if (inAirMaintainXVelocity)
        {
            if (Mathf.Sign(currentHorizontalMoveSpeed) == Mathf.Sign(targetVelocityX))
            {
                if (Mathf.Abs(currentHorizontalMoveSpeed) > Mathf.Abs(targetVelocityX))
                {
                    targetVelocityX = currentHorizontalMoveSpeed;
                }
            }
            else if (Mathf.Sign(currentHorizontalMoveSpeed) != Mathf.Sign(targetVelocityX))
            {
                //if pressing opposite direction of jetpack vector we want to diminish jetpackingVelocity until it reaches 0
                if (Mathf.Abs(currentHorizontalMoveSpeed) > Mathf.Abs(targetVelocityX))
                {
                    currentHorizontalMoveSpeed += targetVelocityX;
                    targetVelocityX = currentHorizontalMoveSpeed;
                }
                else
                {
                    targetVelocityX += currentHorizontalMoveSpeed;
                    currentHorizontalMoveSpeed = 0;
                }
            }
        }


        // The following can be set to a couple of different values depending on what
        // the character is doing. It controls how quickly the character is able to 
        // accelerate.
        float smoothTime = 0;

        if (IsTouchingFloor())
        {
            if (isHoldingSprint)
            {
                smoothTime = accelerationTimeGroundedSprinting;
                targetVelocityX *= sprintSpeedMultiplier;
            }
            else if ((!isHoldingSprint && Mathf.Abs(velocity.x) > Mathf.Abs(directionalInput.x * moveSpeed)))
            {
                //this is so we don't immediately go into walking speed when we let go of sprint
                smoothTime = accelerationTimeGroundedSprinting;
            }
            else
            {
                smoothTime = accelerationTimeGrounded;
            }
            
        }
        else
        {
            smoothTime = accelerationTimeAirborne;
        }

        // Note: I should look up what is being done with the velocityXSmoothing during this part,
        // because I don't understand.
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);

        currentHorizontalMoveSpeed = velocity.x;
    }

    protected virtual void CalculateVerticalVelocity()
    {
        

        // If jumped and has let go of jump, quickly reduce speed
        if (velocity.y > 0 && airborneTimer > minJumpTime && !isHoldingJump)
        {
            velocity.y = velocity.y * 0.5f;
        }

        // If still holding jump, then reduce gravity to give that nice upwards motion feel
        if (velocity.y > 0 && isHoldingJump && airborneTimer >= 0)
        {
            velocity.y += Gravity * .6f * Time.deltaTime;
        }
        else
        {
            velocity.y += Gravity * Time.deltaTime;
        }

        

        
    }

    protected virtual void HandleWallSliding()
    {
        wallDirX = (Controller.collisions.left) ? -1 : 1;

        

        isWallSliding = false;
        // If has wall collision, and is not on the ground, and I'm either at 0 velocity, or I'm moving down, then, enact.
        if (IsTouchingWall() && !IsTouchingFloor() && airborneTimer > minJumpTime)
        {

            isWallSliding = true;
            isHoldingJump = false;

            // Without this, it's possible to be hugging the wall with your back.
            
            
            spriteRenderer.flipX = (wallDirX == -1) ? true : false;

            if (timeToWallUnstick > 0)
            {

                velocityXSmoothing = 0;
                velocity.x = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)//if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }




            // Why do I scale the velocity vector? Oh, to slow him down!
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * .7f;
            }
            

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
        }
    }

    public virtual void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
        if (directionalInput.x > 0)
        {
            facingRight = true;
        }
        else if (directionalInput.x < 0)
        {
            facingRight = false;
        }
    }

    public virtual void OnJumpInputDown()
    {
        if (IsTouchingJumpableSurface())
        {

            if (isWallSliding)
            {
                if (wallDirX == directionalInput.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (directionalInput.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {

                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
                
            }
            else
            {
                velocity.y = jumpVelocity;
                if (isHoldingSprint)
                {
                    inAirMaintainXVelocity = true;
                }
            }
            
            isHoldingJump = true;
            airborneTimer = 0;
            Controller.collisions.below = Controller.collisions.left = Controller.collisions.right = Controller.collisions.above = false;
            canDoubleJump = true;
        }
        else if ((!IsTouchingJumpableSurface() && canDoubleJump))
        {
            TriggerDoubleJumpAnim(); 
            velocity.y = jumpVelocity;
            canDoubleJump = false;
            isHoldingJump = true;
        }
    }

    public virtual void OnJumpInputUp()
    {
        isHoldingJump = false;
    }

    public virtual void OnSprintInputHold()
    {
        if (IsTouchingFloor() && directionalInput.x != 0)
        {
            isHoldingSprint = true;
        }
        else
        {
            isHoldingSprint = false;
        }
    }

    public virtual void OnSprintInputUp()
    {
        isHoldingSprint = false;
    }

    public virtual void OnDodgeInputDown(bool dodgeRight)
    {
        // Dodge should be timer based...
        
        if ((canDodgeRight && dodgeRight) || (canDodgeLeft && !dodgeRight))
        {
            isDodging = true;
            isInvulnerable = true;
            isHoldingJump = true;
            inAirMaintainXVelocity = false;
            isHoldingSprint = false;
            dodgeTimer = 0;
            airborneTimer = 0;
            Controller.collisions.below = Controller.collisions.right = Controller.collisions.left = false;
            velocity.y = dodgeVelocity.y;

            if ((facingRight && !dodgeRight) || (!facingRight && dodgeRight))
            {
                //anim.SetTrigger(dodgeReveresedTriggerHash);
                TriggerDodgeReversedAnim();
            }
            else
            {
                //anim.SetTrigger(dodgeTriggerHash);
                TriggerDodgeAnim();
            }

            if (dodgeRight)
            {
                isDodgingRight = true;
                canDodgeRight = false;
            }
            else
            {
                isDodgingRight = false;
                canDodgeLeft = false;
            }
        }
    }

    public virtual void CaclulateDodgeHorizontalMovement()
    {
        if (dodgeTimer < invulnerableDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
        else if(dodgeTimer >= invulnerableDuration && isInvulnerable)
        {
            isInvulnerable = false;
            spriteRenderer.enabled = true;
        }

        if (dodgeTimer < dodgeDuration)
        {
            velocity.x = (isDodgingRight) ? dodgeVelocity.x : -dodgeVelocity.x;
            dodgeTimer += Time.deltaTime;
            
        }
        else
        {
            isDodging = false;
            dodgeTimer = -1;
            isHoldingJump = false;
            spriteRenderer.enabled = true;
        }
    }

    protected virtual void GetKnockedBack(bool knockedRight)
    {
        isInvulnerable = true;
        isHoldingJump = true;
        inAirMaintainXVelocity = false;
        isHoldingSprint = false;
        knockedBackTimer = 0;
        isBeingKnockedBack = true;
        knockedBackToTheRight = knockedRight;
        Controller.collisions.below = Controller.collisions.right = Controller.collisions.left = false;
        velocity.y = knockedBackVelocity.y;
    }

    protected virtual void CalculateKnockedBackHorizontalMovement()
    {
        knockedBackTimer += Time.deltaTime;
        spriteRenderer.enabled = !spriteRenderer.enabled;
        if (knockedBackTimer < knockedBackDuration)
        {
            velocity.x = (knockedBackToTheRight) ? knockedBackVelocity.x : -knockedBackVelocity.x;
        }
        else
        {
            isBeingKnockedBack = false;
            spriteRenderer.enabled = true;
            isHoldingJump = false;
            knockedBackTimer = -1;
            isInvulnerable = false;
        }
    }

    protected virtual bool CheckForIdleAnim()
    {
        

        if (IsTouchingFloor() && directionalInput.x == 0)
        {
            anim.SetTrigger(idleTriggerHash);
            return true;
        }

        return false;
    }

    protected virtual bool CheckForWalkAnim()
    {

        if (IsTouchingFloor() && directionalInput.x != 0 && !isHoldingSprint)
        {
            anim.SetTrigger(walkTriggerHash);
            return true;
        }

        return false;
    }

    protected virtual bool CheckForJumpAnim()
    {
        
        if (!IsTouchingJumpableSurface())
        {
            anim.SetTrigger(jumpTriggerHash);
            return true;
        }
        return false;
    }

    protected virtual bool CheckForWallSlidingAnim()
    {
        if (IsTouchingWall() && !IsTouchingFloor())
        {
            anim.SetTrigger(wallSlideTriggerHash);
            return true;
        }
        else return false;
    }

    protected virtual  bool CheckForSprintingAnim()
    {
        if (IsTouchingFloor() && isHoldingSprint)
        {
            
            anim.SetTrigger(sprintTriggerHash);
            return true;
        }
        return false;
    }

    // Note that these animations are only triggered during specific actions, so it's easier just 
    // to trigger it within the specific spot of code that executes the action instead of checking for it later. 
    //That's why these are a little different than the other animation triggers.

    protected virtual void TriggerDoubleJumpAnim()
    {
        
        anim.SetTrigger(doubleJumpTriggerHash);
    }

    protected virtual void TriggerDodgeAnim()
    {
        anim.SetTrigger(dodgeTriggerHash);
    }

    protected virtual void TriggerDodgeReversedAnim()
    {
        anim.SetTrigger(dodgeReveresedTriggerHash);
    }
    
    protected virtual void TriggerDeathAnim()
    {
        anim.SetTrigger(deathTriggerHash);
    }

    protected virtual void TriggerResetAnim()
    {
        anim.SetTrigger(resetTriggerHash);
    }


    protected virtual void SetSprite()
    {
        if (!isWallSliding)
        {
            if (facingRight)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
        
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //stateInfo.ToString();



        CheckForIdleAnim();
        CheckForWalkAnim();
        CheckForJumpAnim();
        //CheckForLandingAnim(stateInfo);
        CheckForWallSlidingAnim();
        CheckForSprintingAnim();
    }

    //protected virtual void OnTriggerEnter2D(Collider2D other)
    //{
    //    // This doesn't do anything now... It's just a placeholder.
    //    // If dodging, then temporary objects that could normally cause damage (such as shots)
    //    // will not deal damage. However, stationary hazards such as lava, spike traps, etc...
    //    // should still deal damage.
        

    //    if (isDodging)
    //    {
    //        if (TagsThatCanCauseDamage.Contains(other.tag))
    //        {
                
    //        }
    //    }
    //}
}