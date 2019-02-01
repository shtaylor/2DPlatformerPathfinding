using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using n = NodeTypeEnumerations;
/// <summary>
/// Author: Shane Taylor
/// Adapted from Mat Buckland's C++ Graph class, in the book "Programming Game AI by Example"
/// </summary>
public class GraphEdge
{
    //all of the nodes will be enumerated, so these ints will specify which nodes the edge is connected to
    //all edges are directed edges
    public int from;
    public int to;
    public float cost;


    //constructors
    public GraphEdge(int from, int to, float cost)
    {
        this.from = from;
        this.to = to;
        this.cost = cost;
    }

    public GraphEdge(int from, int to)
    {
        cost = 1;
        this.from = from;
        this.to = to;
    }

    public GraphEdge()
    {
        from = n.INVALID_NODE_INDEX;
        to = n.INVALID_NODE_INDEX;
        cost = 1;
    }

    public GraphEdge(GraphEdge e)
    {
        from = e.from;
        to = e.to;
        cost = e.cost;
    }

    public override bool Equals(object obj)
    {
        
        if (obj is GraphEdge)
        {
            GraphEdge e = (GraphEdge)obj;
            return (e.cost == this.cost && e.from == this.from && e.to == this.to);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        //I'm not sure if I'll need this.
        return base.GetHashCode();
    }
}