using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// This class waits for all of the terrains to finish doing their setup, 
/// then sets up the NavGraphPlatformer once everything is done. It also
/// gives the floornodes a list of applicable action nodes to check for
/// intersections. Once a floor node has been 'split' by an action node,
/// then that action node is pruned from the list of nodes to check,
/// thus optimizing the checking process for the floor nodes.
/// </summary>
[RequireComponent(typeof(NavGraphPlatformer))]
public class NavGraphPreLauncher : MonoBehaviour
{
    bool readyToStart = false;

    bool hasSetupGraph = false;
    GameObject[] allNodeGameObjects;

    // Used for sorting the action nodes.
    SimplePriorityQueue<GameObject, float> actionNodePQ;

    // The floor nodes need to check for intersections with the action nodes so that they can
    // split themselves.
    List<GameObject> sortedActionNodeGameObjects;

    // Having a list of floats will be much faster to work with than loading up the
    // game objects and reading their data for all of the floor nodes.
    List<float> sortedActionNodeXPositions;
    
    
    List<TerrainFloorNodeContainer> allTerrains;

    NavGraphPlatformer graph;

    private void Awake()
    {

        graph = GetComponent<NavGraphPlatformer>();
        if (graph == null)
        {
            Debug.Log("graph is null");
        }
        GameObject[] terrainGameObjects = GameObject.FindGameObjectsWithTag("Terrain");
        allTerrains = new List<TerrainFloorNodeContainer>();

        foreach (GameObject t in terrainGameObjects)
        {
            TerrainFloorNodeContainer temp = t.GetComponent<TerrainFloorNodeContainer>();
            if (temp != null)
            {
                allTerrains.Add(temp);

            }
        }

        ///////////////////////////////////////////////////////////////////////

        actionNodePQ = new SimplePriorityQueue<GameObject, float>();

        allNodeGameObjects = GameObject.FindGameObjectsWithTag("Node");


        sortedActionNodeGameObjects = new List<GameObject>();

        sortedActionNodeXPositions = new List<float>();

        foreach (GameObject nodeGameObject in allNodeGameObjects)
        {
            ActionNode actionNode = nodeGameObject.GetComponent<ActionNode>();
            if (actionNode != null)
            {
                actionNodePQ.Enqueue(nodeGameObject, nodeGameObject.transform.position.x);
            }
        }


        while (actionNodePQ.Count > 0)
        {
            sortedActionNodeGameObjects.Add(actionNodePQ.First);
            sortedActionNodeXPositions.Add(actionNodePQ.Dequeue().transform.position.x);
        }


    }

    // Suppose for example that the left side of a node is at x = -35.
    // Then, this function will return a list of all action node game objects that have
    // position greater than x = -35, because these are the only nodes that the floor nodes
    // are going to need to check for intersections.
    public List<GameObject> GetListOfActionNodesToCheck(float leftOfNodePositionX)
    {

        if (sortedActionNodeGameObjects.Count != sortedActionNodeXPositions.Count)
        {
            Debug.Log("Uneven list sizes. Exiting");
            return null;
        }

        int lowerIndexBound = FindIndexOfGreatestPositionLessThanTarget(leftOfNodePositionX);
        if (lowerIndexBound == -1)
        {
            //Debug.Log("lowerIndexBound = -1");
            return null;
        }
        else
        {
            List<GameObject> tempList = new List<GameObject>();
            for (int i = lowerIndexBound; i < sortedActionNodeGameObjects.Count; i++)
            {
                tempList.Add(sortedActionNodeGameObjects[i]);
            }
            return tempList;
        }

    }

    /// <summary>
    /// This is pretty much a binary search.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public int FindIndexOfGreatestPositionLessThanTarget(float target)
    {
        
        int upperCutPoint = sortedActionNodeXPositions.Count - 1;
        int lowerCutPoint = 0;
        int midPoint = 0;
        bool foundTarget = false;
        if (sortedActionNodeXPositions.Count <= 0)
        {
            return -1;
        }

        while (!foundTarget)
        {
            //midPoint = Mathf.FloorToInt((lowerCutPoint + upperCutPoint) / 2);
            midPoint = Mathf.CeilToInt((lowerCutPoint + upperCutPoint) / 2);

            if (target == sortedActionNodeXPositions[midPoint] || midPoint == sortedActionNodeXPositions.Count - 1 || midPoint == 0)
            {

                return midPoint;
            }
            else if (sortedActionNodeXPositions[midPoint] > target)
            {
                // Check the right half.
                if (sortedActionNodeXPositions[midPoint - 1] <= target)
                {
                    return midPoint - 1;
                }
                upperCutPoint = midPoint - 1;
            }
            else
            {
                if (sortedActionNodeXPositions[midPoint + 1] == target)
                {
                    return midPoint + 1;
                }
                else if (sortedActionNodeXPositions[midPoint + 1] > target)
                {
                    return midPoint;
                }
                else
                {
                    lowerCutPoint = midPoint + 1;
                }
            }
        }
        return -1;
    }

    // Update is called once per frame
    private void Update()
    {
        // Check condition for delayed setup
        if (!readyToStart)
        {
            readyToStart = CheckReadyToStart();
        }
        else if (readyToStart && !hasSetupGraph)
        {
            hasSetupGraph = true;
            graph.SetupGraph();
            foreach (TerrainFloorNodeContainer t in allTerrains)
            {
                Destroy(t);
            }
            GameObject.Destroy(this);

            
        }
    }

    private bool CheckReadyToStart()
    {
        Debug.Log("allTerrains.Count = " + allTerrains.Count);
        if (allTerrains.Count < 1)
        {
            return true;
        }

        foreach (TerrainFloorNodeContainer t in allTerrains)
        {
            if (!t.ready)
            {
                return false;
            }
        }

        return true;
    }

    public void RemoveIntersectorFromActionNodeList(GameObject intersector)
    {
        Vector3 position = intersector.transform.position;

        int index = 0;
        bool found = false;
        while (!found && index < sortedActionNodeGameObjects.Count)
        {

            if (sortedActionNodeGameObjects[index].transform.position == position)
            {
                found = true;
            }

            if (!found)
            {
                index++;
            }
        }

        if (found)
        {
            sortedActionNodeGameObjects.RemoveAt(index);
            sortedActionNodeXPositions.RemoveAt(index);
        }
    }
}
