using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
/// <summary>
/// Author: Shane Taylor
/// Returns the minimum spanning tree of the graph G.
/// </summary>
public class Graph_MST<G> where G:SparseGraph<GraphNode,GraphEdge>
{

    //two approaches
    // 1 - Kruskal's algorithm, keep choosing the cheapest edge, as long as it doesn't make a cycle
    // 2 - Prim's algorithm, at each step choose the cheapest edge that connects the current tree to a non-visited node
    //     We'll be using Prim's, since we don't want to worry about checking cycles.
        
    // a reference to the graph we'll be operating over
    G graph;
    
    // 
    List<GraphEdge> spanningTree;
    List<int> visitedNodes;
    

    public Graph_MST(G graph) : this(graph, Mathf.RoundToInt(Random.Range(0, graph.NumNodes()))){}
    
    public Graph_MST(G graph, int startingNode)
    {
        this.graph = graph;
        spanningTree = new List<GraphEdge>();
        visitedNodes = new List<int>();
        Generate(startingNode);
    }

    public void Generate(int startingNode)
    {
        // We push our graph edges onto the priority queue
        SimplePriorityQueue<GraphEdge, float> pQ = new SimplePriorityQueue<GraphEdge, float>();

        // Create dummy edge
        GraphEdge dummy = new GraphEdge(startingNode, startingNode);

        // Push dummy edge onto priority queue
        pQ.Enqueue(dummy, dummy.cost);

        bool lookingForNewNode;

        //start meat of algorithm
        while (visitedNodes.Count < graph.NumNodes())
        {
            lookingForNewNode = true;
            GraphEdge curEdge = new GraphEdge();
            //dequeue pQ until we find an edge with edge.to not in visitedNodes
            while (lookingForNewNode)
            {
                curEdge = pQ.Dequeue();

                // has the destination of this edge been visited?
                // if not, then we will proceed
                if (!visitedNodes.Contains(curEdge.to))
                {
                    lookingForNewNode = false;
                }
            }
            if (curEdge.from != curEdge.to)
            {
                spanningTree.Add(new GraphEdge(curEdge));
            }
            

            // check the neighbors of the destination of the current Edge
            foreach (GraphEdge edge in graph.edges[curEdge.to])
            {
                if (!visitedNodes.Contains(edge.to))
                {
                    pQ.Enqueue(edge, edge.cost);
                }
            }

            // add curEdge.to to visitedNodes
            visitedNodes.Add(curEdge.to);
        }
    }

    public SparseGraph<GraphNode,GraphEdge> GetMST()
    {


        // essentially, make a new graph with the spanning tree list
        SparseGraph<GraphNode, GraphEdge> g = new SparseGraph<GraphNode, GraphEdge>();
        foreach (GraphNode n in graph.Nodes)
        {
            g.AddNode(new GraphNode(g.nextNodeIndex));
        }

        //add all the edges from the other spanning tree
        foreach (GraphEdge e in spanningTree)
        {
            g.AddDoubleEdge(new GraphEdge(e.from,e.to,e.cost));
        }

        return g;
    }
}
