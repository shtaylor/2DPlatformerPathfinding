using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using n = NodeTypeEnumerations;
/// <summary>
/// Author: Shane Taylor
/// Adapted from Mat Buckland's C++ Graph class, in the book "Programming Game AI by Example"
/// </summary>
public class GraphNode
{

    /*
    a graph node has an index,
     */

    public int index;

    public GraphNode()
    {
        index = n.INVALID_NODE_INDEX;
    }

    public GraphNode(int index)
    {
        this.index = index;
    }


}