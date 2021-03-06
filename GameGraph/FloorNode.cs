﻿using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

/// <summary>
/// Author: Shane Taylor
/// Note: Objects with this script are automatically generated by the
/// TerrainFloorNodeContainer script, which is attached to all Ferr2D terrain
/// objects. You can still use this script without the Ferr2D enabled features,
/// but you would have to manually place your floor nodes. Not recommended.
/// 
/// Objects with this script are instantiated, and they check for intersections with
/// ActionNodes, which are the specific nodes that deliver messages for jumping, dodging,
/// etc... If they encounter such an intersection, they 'split' themselves around the
/// intersecting ActionNodes and link them all together, making them neighbors
/// in the abstract mathematical graph. This feature makes it so you don't have to
/// manually set the edges between all the various nodes in the environment.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class FloorNode : Node {

    
    Vector3 leftBoundPosition;
    Vector3 rightBoundPosition;

    NavGraphPreLauncher navGraphPreLauncher;

    public GameObject floorNodePrefab;

    public TerrainFloorNodeContainer owner;

    public bool instantiatedByCode = true;

    private bool hasCheckedForIntersection = false;

    private float width;

    // Angle is in radians.
    private float angle;
    public bool HasCheckedForIntersection
    {
        get
        {
            return hasCheckedForIntersection;
        }
    }

    public override void Awake()
    {
        base.Awake();
        //walkNodePrefab = Resources.Load("Prefabs/WalkNodePrefab") as GameObject;
        floorNodePrefab = Resources.Load("Prefabs/FloorNodePrefab") as GameObject;
        SetLeftBoundAndRightBoundPositions();
        navGraphPreLauncher = GameObject.FindGameObjectWithTag("Graph").GetComponent<NavGraphPreLauncher>();
        if (navGraphPreLauncher == null)
        {
            Debug.Log("Couldn't find NavGraphPreLauncher");
        }

        nodeType = GameNodeTypes.FloorNode;
    }

    private void Start()
    {
        if (!instantiatedByCode)
        {
            CheckIntersections();
        }
    }

    public void SetLeftBoundAndRightBoundPositions()
    {
        Vector2 size = GetComponent<BoxCollider2D>().size;

        width = size.x*transform.localScale.x/2;
        angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

        float deltaW = width * Mathf.Cos(angle);
        float deltaY = width * Mathf.Sin(angle);
        leftBoundPosition = new Vector3(
            transform.position.x - deltaW, 
            transform.position.y - deltaY);

        rightBoundPosition = new Vector3(
            transform.position.x + deltaW,
            transform.position.y + deltaY);

    }

    public override void SetWeights()
    {
        base.SetWeights();

        float width = GetComponent<BoxCollider2D>().size.x * transform.localScale.x;

        for (int i = 0; i < neighbors.Count; i++)
        {
            // I need to be more specific with the speed that I use to make this calculation
            weights.Add(width / 15);
        }
    }

    /// <summary>
    /// Note that because the game is in 2D, only ActionNodes with center to the right of
    /// the left bound of the floornode need to be checked.
    /// </summary>
    public void CheckIntersections()
    {
        
        List<GameObject> actionNodesToCheck = navGraphPreLauncher.GetListOfActionNodesToCheck(leftBoundPosition.x);
        if (actionNodesToCheck != null)
        {
            for (int i = 0; i < actionNodesToCheck.Count; i++)
            {
                GameObject intersector = actionNodesToCheck[i];
                if (GetComponent<Collider2D>().bounds.Intersects(intersector.GetComponent<Collider2D>().bounds))
                {
                    OnFoundIntersection(intersector, intersector.transform.localScale.x);
                    break;
                }
            }
        }
        hasCheckedForIntersection = true;
    }


    // Code to split current node (blue) with intersecting node (red). 
    // Each shape is a rectangle
    // The current node's next shape I will refer to as Transformation
    // The new node will be referred to as Instantiated
    public void OnFoundIntersection(GameObject intersector, float intersectorWidth)
    {
        // Code to split current node (blue) with intersecting node (red). 
        // Each shape is a rectangle
        // The current node's next shape I will refer to as Transformation
        // The new node will be referred to as Instantiated
        float widthRed = intersector.transform.localScale.x;

        Vector3 centerRed = intersector.transform.position;
        Vector3 centerBlue = transform.position;

        // the red node's horizontal dimension is parallel to the blue node's.
        float transformationWidth = Mathf.Max(Vector2.Distance(centerRed, leftBoundPosition) - .5f * widthRed, 0);
        float instantiatedWidth = Mathf.Max(Vector2.Distance(rightBoundPosition, centerRed) - .5f * widthRed, 0);

        Vector3 transformationMidpoint = new Vector3(leftBoundPosition.x + .5f * transformationWidth * Mathf.Cos(angle),
            leftBoundPosition.y + .5f * transformationWidth * Mathf.Sin(angle),
            0);
        Vector3 instantiatedMidpoint = new Vector3(rightBoundPosition.x - .5f * instantiatedWidth * Mathf.Cos(angle),
            rightBoundPosition.y - .5f * instantiatedWidth * Mathf.Sin(angle),
            0);

        // now, relocate & resize current node (it will become the left half)
        transform.localScale = new Vector3(transformationWidth, transform.localScale.y, transform.localScale.z);
        transform.SetPositionAndRotation(transformationMidpoint, transform.rotation);

        // Instantiate right half of blue node.
        GameObject floorNodeGameObject = Instantiate(floorNodePrefab);
        floorNodeGameObject.transform.localScale = new Vector3(instantiatedWidth, transform.localScale.y, transform.localScale.z);
        floorNodeGameObject.transform.SetPositionAndRotation(instantiatedMidpoint, transform.rotation);
        FloorNode newFloorNode = floorNodeGameObject.AddComponent<FloorNode>();

        // Terrain that owns this floorNode needs to have its list of floor nodes updated.
        // I believe that this will ensure that all of the instantiated nodes will be checked prior to
        // the terrain giving the all-go.
        if (owner!=null)
        {
            newFloorNode.owner = owner;
            newFloorNode.owner.ownedFloorNodes.Add(this);
        }
        

        ActionNode intersectingNode = intersector.GetComponent<ActionNode>();

        intersectingNode.SetLeftNeighbor(this);//gameObject.GetComponent<NG2D_WalkNode>()
        intersectingNode.SetRightNeighbor(newFloorNode);
        newFloorNode.SetLeftNeighbor(intersectingNode);

        Node rightNeighbor = GetRightNeighbor();
        if (rightNeighbor != null)
        {
            newFloorNode.SetRightNeighbor(rightNeighbor);
            rightNeighbor.SetLeftNeighbor(newFloorNode);
        }
        SetRightNeighbor(intersectingNode);
        navGraphPreLauncher.RemoveIntersectorFromActionNodeList(intersector);
        // NOTE: I must implement the above function before I can uncomment the line below.
        newFloorNode.CheckIntersections();
    }
}
