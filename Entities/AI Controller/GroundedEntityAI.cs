using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using N = NumericalExtension;
using C = GroundedEntityAIConstants;
using UnityEngine;

/// <summary>
/// Author: Shane Taylor
/// This is the main class that handles how the invidual AI-controlled
/// entities navigate the environment. They essentially get a list of messages
/// to enact in the event that they encounter specific nodes that are 
/// automatically generated around the environment. The graph determines which
/// messages to give the entity depending on where they are and where they're
/// trying to go.
/// </summary>
[RequireComponent(typeof(GroundedEntity))]
public class GroundedEntityAI : MonoBehaviour
{
    public DixieController groundedEntity;

    public NavGraphPlatformer graph;

    public StateMachine<GroundedEntityAI> navFSM;

    public StateMachine<GroundedEntityAI> localNavFSM;

    public Message currentMessage;

    public List<int> path;

    public Dictionary<int, Message> pathMessageLookup;

    public int mostRecentNode;
    public int nextNodeInPath;


    public SimplePriorityQueue<Message, int> messagePriorityQueue;

    public bool isSearching;

    public float jumpTimer = -1;
    public float jumpTimeFilter = .05f;
    
    public float timeBetweenSearches = 1;

    float maxDistanceToSeekPlayer = 50;

    //public GameObject chit;

    //public GameObject JumpNodeContainer;

    //public GroundedEntityAI_PathOptimization pathOptimization;

    private void Awake()
    {
        graph = GameObject.FindGameObjectWithTag("Graph").GetComponent<NavGraphPlatformer>();
        groundedEntity = GetComponent<DixieController>();
        navFSM = new StateMachine<GroundedEntityAI>(this);
        messagePriorityQueue = new SimplePriorityQueue<Message, int>();
        navFSM.ChangeState(new DoNothing());

        navFSM.previousState = new DoNothing();


        StartCoroutine(UpdatePath());

        timeBetweenSearches += Random.Range(0, .1f);
        //pathOptimization = GameObject.FindGameObjectWithTag("Graph").GetComponent<GroundedEntityAI_PathOptimization>();

        //pathOptimization.QueueAddDixie(this);
    }



    private IEnumerator UpdatePath()
    {
        yield return new WaitForSeconds(timeBetweenSearches);

        
        if (isSearching
            && !path.Contains(graph.playerLastNodeID) 
            && graph.playerLastNodeID != -1 
            && mostRecentNode != graph.playerLastNodeID 
            && groundedEntity.IsTouchingFloor())
        {
            path = graph.GetPathIDs(mostRecentNode, graph.playerLastNodeID);

            pathMessageLookup = graph.GetPathMessages(path);

            messagePriorityQueue.Clear();
            messagePriorityQueue.Enqueue(pathMessageLookup[mostRecentNode], -1000);
            //isSearching = true;
        }
        
        StartCoroutine(UpdatePath());
    }

    // If I'm not using the separate path optimization class I can remove this function.
    public void TempUpdatePath(List<int> pathToPlayer, Dictionary<int,Message> messages)
    {
        path = pathToPlayer;
        pathMessageLookup = messages;
        messagePriorityQueue.Clear();
        messagePriorityQueue.Enqueue(pathMessageLookup[mostRecentNode], -1000);
        isSearching = true;
    }

    private void Update()
    {
        
        if (navFSM.currentState != null)
        {
            navFSM.Update();
        }

        if (mostRecentNode == graph.playerLastNodeID )
        {
            
            //groundedEntity.SetDirectionalInput(Vector2.zero);
            //navFSM.ChangeState(new DoNothing());
            if (graph.player.IsTouchingFloor() || graph.player.isDead)
            {
                isSearching = false;
                if (graph.player.transform.position.x < transform.position.x)
                {
                    // Walk Left.
                    if (!(navFSM.currentState is Move) || (navFSM.currentState is Move && groundedEntity.directionalInput.x != -1))
                    {
                        navFSM.ChangeState(new Move(false));
                    }
                }
                else if (graph.player.transform.position.x >= transform.position.x)
                {
                    // Walk Right.
                    if (!(navFSM.currentState is Move) || (navFSM.currentState is Move && groundedEntity.directionalInput.x != 1))
                    {
                        navFSM.ChangeState(new Move(true));
                    }
                }
            }
            else
            {
                navFSM.ChangeState(new DoNothing());
            }
        }
        else
        {
            isSearching = true;
        }
        
        if (messagePriorityQueue.Count > 0)
        {
            DealWithMessages();
        }
        
    }

    private void DealWithMessages()
    {
        
        Message msg = messagePriorityQueue.Dequeue();
        currentMessage = msg;
        messagePriorityQueue.Clear();

        switch (currentMessage)
        {
            case Message.WalkLeft:
                navFSM.ChangeState(new Move(false));
                break;
            case Message.WalkRight:
                navFSM.ChangeState(new Move(true));
                break;
            case Message.DoNothing:
                navFSM.ChangeState(new DoNothing());
                break;
            case Message.DodgeLeft:
                navFSM.ChangeState(new Dodge(false));
                break;
            case Message.DodgeRight:
                navFSM.ChangeState(new Dodge(true));
                break;
            case Message.Jump5:
                groundedEntity.SetDirectionalInput((groundedEntity.velocity.x>0) ? new Vector2(-1,0) : new Vector2(1,0));
                navFSM.ChangeState(new JumpStraightUp(C.j5));
                break;
            case Message.Jump3:
                groundedEntity.SetDirectionalInput((groundedEntity.velocity.x > 0) ? new Vector2(-1, 0) : new Vector2(1, 0));
                navFSM.ChangeState(new JumpStraightUp(C.j3));
                break;
            case Message.JumpLeft1:
                navFSM.ChangeState(new Jump(false, C.j1));
                break;
            case Message.JumpLeft2:
                navFSM.ChangeState(new Jump(false, C.j2));
                break;
            case Message.JumpLeft3:
                navFSM.ChangeState(new Jump(false, C.j3));
                break;
            case Message.JumpLeft4:
                navFSM.ChangeState(new Jump(false, C.j4));
                break;
            case Message.JumpLeft5:
                navFSM.ChangeState(new Jump(false, C.j5));
                break;
            case Message.JumpRight1:
                navFSM.ChangeState(new Jump(true, C.j1));
                break;
            case Message.JumpRight2:
                navFSM.ChangeState(new Jump(true, C.j2));
                break;
            case Message.JumpRight3:
                navFSM.ChangeState(new Jump(true, C.j3));
                break;
            case Message.JumpRight4:
                navFSM.ChangeState(new Jump(true, C.j4));
                break;
            case Message.JumpRight5:
                navFSM.ChangeState(new Jump(true, C.j5));
                break;
            case Message.Recalculate:
                navFSM.ChangeState(new Recalculate());
                break;
            default:
                break;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.CompareTag("Node"))
        {
            
            Node node = other.gameObject.GetComponent<Node>();

            if (!isSearching)
            {
                mostRecentNode = node.ID;
            }

            if (path == null)
            {
                mostRecentNode = node.ID;
                return;
            }

            if (path.Contains(node.ID) && node.ID != nextNodeInPath && node.nodeType == GameNodeTypes.DoubleJump)
            {
                return;
            }

            if (path.Contains(node.ID) && isSearching)
            {
                int indexOfCurrentNode = path.IndexOf(node.ID);
                mostRecentNode = node.ID;
                // Trying to make a system so that if the node doesn't equal the next node
                // and it is a DoubleJump type node, then ignore it.
                //Debug.Log("current Node = " + mostRecentNode);
                
                if (indexOfCurrentNode != path.Count - 1)
                {
                    nextNodeInPath = path[indexOfCurrentNode + 1];
                    //Debug.Log("Next Node = " + path[indexOfCurrentNode + 1]);
                }

                if (navFSM.currentState is Move)
                {
                    if (groundedEntity.directionalInput.x == 1 && pathMessageLookup[node.ID] == Message.WalkRight)
                    {
                        return;
                    }
                    else if (groundedEntity.directionalInput.x == -1 && pathMessageLookup[node.ID] == Message.WalkLeft)
                    {
                        return;
                    }
                }

                messagePriorityQueue.Enqueue(pathMessageLookup[node.ID], -path.IndexOf(node.ID));
            }
            else if (!path.Contains(node.ID) && node.nodeType == GameNodeTypes.DoubleJump)
            {
                return;
            }
            else if(isSearching && (jumpTimer == -1 || jumpTimer >= jumpTimeFilter ))
            {
                mostRecentNode = node.ID;
                messagePriorityQueue.Enqueue(Message.Recalculate, int.MaxValue);
            }
        }
    }

    public void PrintPath(List<int> path)
    {
        Debug.Log("Path:");
        foreach (var n in path)
        {
            Debug.Log(n);
        }
    }
}

public class KnockedBack : GroundedEntityState
{

}

public class DoNothing : GroundedEntityState
{
    public DoNothing()
    {
        this.duration = 0;
    }
    public DoNothing(float duration)
    {
        this.duration = Mathf.Abs(duration);
        timer = 0;
    }

    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        entityAI.groundedEntity.OnSprintInputUp();
        entityAI.isSearching = false;
    }

    public override void Execute(GroundedEntityAI entityAI)
    {
        base.Execute(entityAI);
        entityAI.groundedEntity.SetDirectionalInput(Vector2.zero);
        if (duration > 0)
        {
            if (timer > duration && entityAI.navFSM.previousState != null)
            {
                entityAI.navFSM.ChangeState(entityAI.navFSM.previousState);
            }
            timer += Time.deltaTime;
        }
    }
}

public class Move : GroundedEntityState
{
    
    
    public Move(bool direction)
    {
        this.direction = direction;
        this.holdingSprint = false;


    }

    public Move(bool direction, bool holdingSprint)
    {
        this.direction = direction;
        this.holdingSprint = holdingSprint;
    }

    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        if (holdingSprint)
        {
            entityAI.groundedEntity.OnSprintInputHold();
        }
        else
        {
            entityAI.groundedEntity.OnSprintInputUp();
        }
        SetDirectionalVector();
    }

    public override void Execute(GroundedEntityAI entityAI)
    {
        base.Execute(entityAI);
        entityAI.groundedEntity.SetDirectionalInput(directionalVector);

        
    }

    public override void Exit(GroundedEntityAI entityAI)
    {
        base.Exit(entityAI);
        //entityAI.groundedEntity.OnSprintInputUp();
    }

    
}

public class JumpStraightUp : GroundedEntityState
{
    bool hasJumped = false;

    public JumpStraightUp(float duration)
    {
        this.duration = duration;
    }
    
    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        entityAI.groundedEntity.OnSprintInputUp();

        this.directionalVector = Vector2.zero;
        entityAI.jumpTimer = 0;
    }

    public override void Execute(GroundedEntityAI entityAI)
    {
        base.Execute(entityAI);

        entityAI.jumpTimer += Time.deltaTime;

        entityAI.groundedEntity.SetDirectionalInput(directionalVector);

        //if (!entityAI.groundedEntity.IsTouchingJumpableSurface())
        //{
        //    GameObject tempChit = GameObject.Instantiate<GameObject>(entityAI.chit, entityAI.transform.position, entityAI.transform.rotation);
        //    tempChit.transform.parent = entityAI.JumpNodeContainer.transform;
        //}
        
        if (!hasJumped)
        {
            timer = 0;
            entityAI.groundedEntity.OnJumpInputDown();
            hasJumped = true;
        }
        if (timer >= duration)
        {
            entityAI.groundedEntity.OnJumpInputUp();

        }
        if (hasJumped)
        {
            timer = timer + Time.deltaTime;
        }
    }

    public override void Exit(GroundedEntityAI entityAI)
    {
        base.Exit(entityAI);
        entityAI.jumpTimer = -1;
        entityAI.groundedEntity.OnJumpInputUp();
    }
}

public class Jump : GroundedEntityState
{

    bool hasJumped = false;

    public Jump(bool direction, float duration)
    {
        this.direction = direction;
        this.duration = duration;
        this.holdingSprint = false;
    }

    public Jump(bool direction, float duration, bool holdingSprint)
    {

        this.direction = direction;
        this.duration = duration;
        this.holdingSprint = holdingSprint;
    }

    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        if ((entityAI.groundedEntity.directionalInput.x == -1 && direction) 
            || (entityAI.groundedEntity.directionalInput.x == 1 && !direction))
        {
            entityAI.groundedEntity.velocity.x = 0;
        }
        
        if (holdingSprint)
        {
            entityAI.groundedEntity.OnSprintInputHold();
        }
        else
        {
            entityAI.groundedEntity.OnSprintInputUp();
        }
        SetDirectionalVector();
        entityAI.jumpTimer = 0;
        
    }

    public override void Execute(GroundedEntityAI entityAI)
    {
        base.Execute(entityAI);


        //if (!entityAI.groundedEntity.IsTouchingJumpableSurface())
        //{
        //    GameObject tempChit = GameObject.Instantiate<GameObject>(entityAI.chit, entityAI.transform.position, entityAI.transform.rotation);
        //    tempChit.transform.parent = entityAI.JumpNodeContainer.transform;
        //}
        if (hasJumped)
        {
            timer = timer + Time.deltaTime;
            entityAI.jumpTimer += Time.deltaTime;
            entityAI.groundedEntity.SetDirectionalInput(directionalVector);
        }
        if (!hasJumped)
        {
            timer = 0;
            entityAI.groundedEntity.SetDirectionalInput(directionalVector);
            entityAI.groundedEntity.OnJumpInputDown();
            hasJumped = true;
        }
        if (timer >= duration)
        {
            entityAI.groundedEntity.OnJumpInputUp();
            
        }
    }

    public override void Exit(GroundedEntityAI entityAI)
    {
        base.Exit(entityAI);
        entityAI.jumpTimer = -1;
        entityAI.groundedEntity.OnJumpInputUp();
    }
}

public class Dodge : GroundedEntityState
{

    bool hasDodged = false;

    public Dodge(bool direction)
    {
        this.direction = direction;
        SetDirectionalVector();
    }

    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        entityAI.jumpTimer = 0;
        entityAI.groundedEntity.OnSprintInputUp();
    }

    public override void Execute(GroundedEntityAI entityAI)
    {
        base.Execute(entityAI);
        entityAI.jumpTimer += Time.deltaTime;
        //GameObject tempChit = GameObject.Instantiate<GameObject>(entityAI.chit, entityAI.transform.position, entityAI.transform.rotation);
        //tempChit.transform.parent = entityAI.JumpNodeContainer.transform;
        if (!hasDodged)
        {
            entityAI.groundedEntity.OnDodgeInputDown(direction);
        }
        entityAI.groundedEntity.SetDirectionalInput(directionalVector);
    }

    public override void Exit(GroundedEntityAI entityAI)
    {
        base.Exit(entityAI);
        entityAI.jumpTimer = -1;
    }
}

public class Recalculate : GroundedEntityState
{
    public override void Enter(GroundedEntityAI entityAI)
    {
        base.Enter(entityAI);
        entityAI.path = entityAI.graph.GetPathIDs(entityAI.mostRecentNode, entityAI.graph.playerLastNodeID);
        entityAI.pathMessageLookup = entityAI.graph.GetPathMessages(entityAI.path);
        entityAI.messagePriorityQueue.Clear();
        entityAI.messagePriorityQueue.Enqueue(entityAI.pathMessageLookup[entityAI.mostRecentNode], -1000);
        entityAI.isSearching = true;
    }
}

// Temporary. Will redesign the jump durations later.
public static class GroundedEntityAIConstants
{
    public static float j1 = .05f;
    public static float j2 = .1f;
    public static float j3 = .2f;
    public static float j4 = .4f;
    public static float j5 = .8f;
}
