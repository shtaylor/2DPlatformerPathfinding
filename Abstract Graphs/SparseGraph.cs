using System.Collections;
using System.Collections.Generic;
using NT = NodeTypeEnumerations;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// Adapted from Mat Buckland's C++ Graph class, in the book "Programming Game AI by Example"
/// Some major modifications were made to optimize for C# versus C++
/// </summary>
public class SparseGraph<N, E> where N : GraphNode where E : GraphEdge
{

    // for easier reference
    public class NodeVector : List<N> { }
    public class EdgeList : LinkedList<E>{ }
    public class EdgeListVector : List<EdgeList>{ }

    //the nodes in this graph
    public NodeVector Nodes { get; set; }

    //the edges in this graph
    public EdgeListVector edges = new EdgeListVector();


    //the index of the next node to be added
    public int nextNodeIndex;

    public SparseGraph()
    {
        Nodes = new NodeVector();
        nextNodeIndex = 0;
    }
    

    /**
     *  @return true if the edge is not present in the graph. Used when adding
     *  edges to prevent duplication
     *  
     */
    private bool UniqueEdge(int from, int to)
    {
        foreach (var e in edges[from])
        {
            if (e.to == to)
            {
                return false;
            }
        }
        return true;
    }

    /**
     *  iterates through all the edges in the graph and removes any that point
     *  to an invalidated node
     */
    private void CullInvalidEdges()
    {
        foreach (EdgeList EL in edges)
        {

            var node = EL.First;

            while (node != null)
            {
                var next = node.Next;
                if (Nodes[node.Value.to].index == NT.INVALID_NODE_INDEX ||
                    Nodes[node.Value.from].index == NT.INVALID_NODE_INDEX)
                {
                    EL.Remove(node);
                }
                node = next;
            }

        }
    }

    /**
     * method for obtaining a reference to a specific node
     * @return the node at the given index
     */
    public N GetNode(int idx)
    {
        if (idx < Nodes.Count && idx >= 0)
        {
            return Nodes[idx];
        }
        else
        {
            Debug.Log("Tried to access non-existant node");
            return null;
        }
    }

    /**
     * method for obtaining a reference to a specific edge
     */
    public E GetEdge(int from, int to)
    {
        if (from < Nodes.Count && from >= 0 && to < Nodes.Count && to >= 0)
        {
            foreach (var e in edges[from])
            {
                if (e.to == to)
                {
                    return e;
                }
            }
            Debug.Log("Didn't find specified edge");
            return null;
        }
        else
        {
            Debug.Log("Invalid from or to");
            return null;
        }
    }

    // for modifying edge weights
    public void MultiplyAllEdgesBetween2Nodes(int from, int to, float multiplier)
    {
        List<E> _edges = new List<E>();
        E _edge = GetEdge(from, to);
        if (_edge != null)
        {
            _edges.Add(_edge);
        }
        _edge = GetEdge(to, from);
        if (_edge != null)
        {
            _edges.Add(_edge);
        }
        foreach (var e in _edges)
        {
            e.cost *= multiplier;
        }
    }

    /**
     * Given a node this method first checks to see if the node has been added
     * previously but is now innactive. If it is, it is reactivated.
     *
     *  If the node has not been added previously, it is checked to make sure its
     *  index matches the next node index before being added to the graph
     */
    public int AddNode(N node)
    {
        //make sure the client is not trying to add a node with the same ID as
        //a currently active node
        if (node.index < Nodes.Count)
        {
            Debug.Assert(Nodes[node.index].index == NT.INVALID_NODE_INDEX, 
                "<SparseGraph::AddNode>: Attempting to add a node with a duplicate ID");
            Nodes[node.index] = node;
            return nextNodeIndex;
        }
        else
        {
            Debug.Assert(node.index == nextNodeIndex, "<SparseGraph::AddNode>:invalid index");
            Nodes.Add(node);
            edges.Add(new EdgeList());
            //this should increment nextNodeIndex to += 1, but return nextNodeIndex-1
            return nextNodeIndex++;
        }
    }

    /** 
     *  Removes a node from the graph and removes any links to neighbouring
     *  nodes
     */
    public void RemoveNode(int node)
    {
        Debug.Assert(node < Nodes.Count, "<SparseGraph::RemoveNode>: invalid node index");
        Nodes[node].index = NT.INVALID_NODE_INDEX;

        CullInvalidEdges();
    }

    /**
     *  Use this to add an edge to the graph. The method will ensure that the
     *  edge passed as a parameter is valid before adding it to the graph. If the
     *  graph is a digraph then a similar edge connecting the nodes in the opposite
     *  direction will be automatically added.
     */
    public void AddEdge(E edge)
    {
        //first make sure the from and to nodes exist within the graph 
        Debug.Assert(edge.from < nextNodeIndex && edge.to < nextNodeIndex, "<SparseGraph::AddEdge>: invalid node index");

        //make sure both nodes are active before adding the edge
        if (Nodes[edge.to].index != NT.INVALID_NODE_INDEX 
            && Nodes[edge.from].index != NT.INVALID_NODE_INDEX)
        {
            //add the edge, first making sure it is unique
            if (UniqueEdge(edge.from, edge.to))
            {
                edges[edge.from].AddLast(edge);
            }
        }
    }

    public void AddDoubleEdge(E edge)
    {
        //first make sure the from and to nodes exist within the graph 
        Debug.Assert(edge.from < nextNodeIndex && edge.to < nextNodeIndex, "<SparseGraph::AddEdge>: invalid node index");

        if (Nodes[edge.to].index != NT.INVALID_NODE_INDEX
            && Nodes[edge.from].index != NT.INVALID_NODE_INDEX)
        {
            AddEdge(edge);
            AddEdge((E) new GraphEdge(edge.to, edge.from, edge.cost));
        }
    }

    /**
     * removes the edge connecting from and to from the graph (if present). If
     * a digraph then the edge connecting the nodes in the opposite direction 
     * will also be removed.
     */
    public void RemoveEdge(int from, int to)
    {
        Debug.Assert(from < Nodes.Count && to < Nodes.Count, "<SparseGraph::RemoveEdge>:invalid node index");
        
        var node = edges[from].First;

        while (node != null)
        {
            var next = node.Next;
            if (node.Value.to == to)
            {
                edges[from].Remove(node);
                break;
            }
            node = next;
        }
    }

    /**
     * Sets the cost of a specific edge
     */
    public void SetEdgeCost(int from, int to, float newCost)
    {
        //make sure from and to are valid
        Debug.Assert(from < Nodes.Count && to < from + 1, "<SparseGraph::SetEdgeCost>: invalid index");

        foreach (var e in edges[from])
        {
            if (e.to == to)
            {
                e.cost = newCost;
            }
        }
    }

    public int NumNodes()
    {
        return Nodes.Count;
    }

    public int NumActiveNodes()
    {
        int count = 0;
        foreach (var n in Nodes)
        {
            if (n.index != NT.INVALID_NODE_INDEX)
            {
                count++;
            }
        }
        return count;
    }

    public int NumEdges()
    {
        int count = 0;
        foreach (var EL in edges)
        {
            count += EL.Count;
        }
        return count;
    }

    public bool IsEmpty()
    {
        if (Nodes.Count == 0)
        {
            return true;
        }
        else
        {
            foreach (var n in Nodes)
            {
                if (n.index != NT.INVALID_NODE_INDEX)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /**
     * returns true if a node with the given index is present in the graph
     */
    public bool IsNodePresent(int id)
    {
        if (id >= Nodes.Count || Nodes[id].index == NT.INVALID_NODE_INDEX)
        {
            return false;
        }
        return true;
    }

    /**
     * @return true if an edge with the given from/to is present in the graph
     */
    public bool IsEdgePresent(int from, int to)
    {
        if (IsNodePresent(from) && IsNodePresent(to))
        {
            foreach (var e in edges[from])
            {
                if (e.to == to)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override string ToString()
    {
        string s = "graph:";
        foreach (var EL in edges)
        {
            foreach (var e in EL)
            {
                s += "\n" + "from: " + e.from + " , to: " + e.to + " , cost: " + e.cost;
            }
        }
        return s;
    }
}