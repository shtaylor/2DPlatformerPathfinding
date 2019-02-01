using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using N = NodeTypeEnumerations;

//Breadth-First Search
public class GraphSearch_BFS<G> where G : SparseGraph<GraphNode, GraphEdge>
{

    const int VISITED = 0;
    const int UNVISITED = 1;
    const int NO_PARENT = 2;

    G graph;



    List<int> route, visitedNodes;

    int source, target;

    public bool found;

    public GraphSearch_BFS(G graph, int source) : this(graph, source, N.INVALID_NODE_INDEX) { }

    public GraphSearch_BFS(G graph, int source, int target)
    {
        this.graph = graph;
        this.source = source;
        this.target = target;
        found = false;
        visitedNodes = new List<int>(this.graph.NumNodes());
        for (int i = 0; i < visitedNodes.Capacity; i++)
        {
            visitedNodes.Add(UNVISITED);
        }
        route = new List<int>(this.graph.NumNodes());
        for (int i = 0; i < visitedNodes.Capacity; i++)
        {
            route.Add(NO_PARENT);
        }

        found = Search();
    }

    public bool Search()
    {
        Queue<GraphEdge> Q = new Queue<GraphEdge>();

        GraphEdge dummy = new GraphEdge(source, source);
        Q.Enqueue(dummy);
        visitedNodes[dummy.to] = VISITED;

        while (Q.Count > 0)
        {
            GraphEdge nextEdge = Q.Dequeue();

            route[nextEdge.to] = nextEdge.from;

            

            if (nextEdge.to == target)
            {
                return true;
            }

            foreach (GraphEdge edge in graph.edges[nextEdge.to])
            {
                if (visitedNodes[edge.to] == UNVISITED)
                {
                    Q.Enqueue(edge);

                    //mark it as visited
                    visitedNodes[nextEdge.to] = VISITED;
                }
            }
        }

        return false;
    }

    public List<int> GetPathToTarget()
    {
        List<int> path = new List<int>();

        if (!found || target < 0)
        {
            return path;
        }

        int node = target;

        while (node != source)
        {
            node = route[node];
            path.Insert(0, node);
        }

        //since I want to see the target node in my list
        path.Add(target);

        return path;
    }
}