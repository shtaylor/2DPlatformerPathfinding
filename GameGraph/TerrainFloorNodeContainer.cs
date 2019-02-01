using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// This class is what enables automatic floor node generation.
/// This class requires the awesome "Ferr2D" terrain tool available
/// on the Unity Asset store. This class plugs into terrain objects,
/// and then generates floornodes wherever the terrain object
/// has an edge marked facing up. If consecutive edges are floor edges,
/// then those floor nodes are linked up, thus setting them as neighbors
/// in the graph.
/// </summary>
public class TerrainFloorNodeContainer : MonoBehaviour {

    public bool ready = false;

    public List<FloorNode> ownedFloorNodes;

    public List<bool> floorNodeHasCheckedForCollisions;

    public Ferr2DPath ferr2DPath;

    public Ferr2DT_PathTerrain terrain;

    private void Start()
    {
        terrain = GetComponent<Ferr2DT_PathTerrain>();
        ferr2DPath = terrain.PathData;
        List<Vector2> rawPath = ferr2DPath.GetPathRaw();
        List<Vector2> finalPath = ferr2DPath.GetFinalPath();

        //List<FloorNode> floorNodeList = new List<FloorNode>(finalPath.Count);
        FloorNode[] floorNodeArray = new FloorNode[finalPath.Count];

        // Create Walk Nodes
        for (int i = 0; i < finalPath.Count; i++)
        {


            

            Ferr2DT_TerrainDirection dir = Ferr2DT_PathTerrain.GetSegmentDirection(ferr2DPath, i);
            if (dir == Ferr2DT_TerrainDirection.Top)
            {
                if (i == finalPath.Count - 1)
                {
                    floorNodeArray[i] = InstantiateFloorNode(finalPath[0], finalPath[i]);
                }
                else
                {
                    floorNodeArray[i] = InstantiateFloorNode(finalPath[i + 1], finalPath[i]);
                }

                floorNodeArray[i].owner = this;
                ownedFloorNodes.Add(floorNodeArray[i]);
            }
        }


        // Check if there are any consecutive walk nodes
        // if so, set them as neighbors
        for (int i = 1; i < floorNodeArray.Length; i++)
        {
            if (floorNodeArray[i] != null && floorNodeArray[i - 1] != null)
            {
                // found consecutive walk nodes
                // set their neighboringness with regards to their x-coordinate
                if (floorNodeArray[i - 1].gameObject.transform.position.x < floorNodeArray[i].gameObject.transform.position.x)
                {
                    floorNodeArray[i - 1].SetRightNeighbor(floorNodeArray[i]);
                    floorNodeArray[i].SetLeftNeighbor(floorNodeArray[i - 1]);
                }
                else
                {
                    floorNodeArray[i].SetRightNeighbor(floorNodeArray[i - 1]);
                    floorNodeArray[i - 1].SetLeftNeighbor(floorNodeArray[i]);
                }
            }
        }

        // the edge case
        if (floorNodeArray[0] != null && floorNodeArray[floorNodeArray.Length - 1] != null)
        {
            if (floorNodeArray[0].gameObject.transform.position.x < floorNodeArray[floorNodeArray.Length - 1].gameObject.transform.position.x)
            {
                floorNodeArray[0].SetRightNeighbor(floorNodeArray[floorNodeArray.Length - 1]);
                floorNodeArray[floorNodeArray.Length - 1].SetLeftNeighbor(floorNodeArray[0]);
            }
            else
            {
                floorNodeArray[floorNodeArray.Length - 1].SetRightNeighbor(floorNodeArray[0]);
                floorNodeArray[0].SetLeftNeighbor(floorNodeArray[floorNodeArray.Length - 1]);
            }
        }

        foreach (FloorNode w in floorNodeArray)
        {
            if (w != null)
            {
                w.CheckIntersections();
            }
        }
    }

    private void Update()
    {
        if (!ready)
        {
            ready = CheckIfReady();
        }
    }

    public FloorNode InstantiateFloorNode(Vector2 u, Vector2 v)
    {
        // get direction/angle
        // get midpoint along vector
        // instantiate nice WalkNodes with dimensions that will stick out the proper amount (height should be 3)

        Vector3 midpoint = new Vector3((u.x + v.x) / 2 + transform.position.x, (u.y + v.y) / 2 + transform.position.y, 0);
        //I'm not sure if this is correct. Assume u is the tail
        float angle = Vector2Extension.DirectionAngle(v - u);

        float width = Vector2.Distance(u, v);

        GameObject floorNodePrefab = Resources.Load("Prefabs/FloorNodePrefab") as GameObject;

        GameObject walkNodeGameObject = Instantiate<GameObject>(floorNodePrefab);
        walkNodeGameObject.transform.SetPositionAndRotation(midpoint, Quaternion.Euler(0, 0, angle));
        walkNodeGameObject.transform.localScale = new Vector3(width, 2, 1);
        return walkNodeGameObject.AddComponent<FloorNode>();

    }

    public bool CheckIfReady()
    {
        foreach (FloorNode floor in ownedFloorNodes)
        {
            if (!floor.HasCheckedForIntersection)
            {
                return false;
            }
        }
        return true;
    }
}