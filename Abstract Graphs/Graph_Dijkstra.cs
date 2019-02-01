using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
/// <summary>
/// Author: Shane Taylor
/// 
/// Performs a search using Dijkstra's algorithm, and returns the list of
/// nodes required to complete the traversal from the starting node to the
/// target node.
/// </summary>
public class Graph_Dijkstra<G> where G : SparseGraph<GraphNode,GraphEdge>
{

    // Description: calculates shortest path between source node and given node.
    // 

    // the graph
    G graph;
    //where am I starting?
    int source;
    //where am I trying to find a path to?
    int target;
    //every node has a constantly updated node that indicates the cumulative distance to the following node
    float[] dist;

    // who points at this node along the path?
    int[] previous;

    //When I visit a node I lock in its distance, until then all graphs have weight (the float) changing and resorting over time.
    SimplePriorityQueue<int, float> Unvisited;

    public Graph_Dijkstra(G _graph, int _source, int _target)
    {
        //get a reference to the graph.
        graph = _graph;

        //we update the distances throughout the algorithm
        dist = new float[graph.NumActiveNodes()];
        for (int i = 0; i < graph.NumActiveNodes(); i++)
        {
            dist[i] = float.PositiveInfinity;
        }

        source = _source;
        target = _target;

        //we pop unvisited and update dist[]
        Unvisited = new SimplePriorityQueue<int, float>();

        // we initialize the unvisited list. We update the distance as we go
        foreach (GraphNode node in graph.Nodes)
        {
            Unvisited.Enqueue(node.index, float.PositiveInfinity);
        }
        
        // we update the priority queue with the source pointing to itself as the best next option.
        Unvisited.UpdatePriority(source, 0);

        // we need to initialize the previous graph, which points at who leads to you.
        previous = new int[graph.NumActiveNodes()];
        for (int i = 0; i < graph.NumActiveNodes(); i++)
        {
            previous[i] = -1;
        }

        //start algorithm
        Search();
    }

    public void Search()
    {
        previous[source] = source;
        //we have to continue until we pop the target
        while (Unvisited.Contains(target)||Unvisited.Count==0)
        {

            // If we don't have any nodes left to find a path to, then stop 
            if (Unvisited.Count ==0)
            {
                return;
            }
            
            //before we mess with it we need to get the distance to the node at the front of the priority queue
            dist[Unvisited.First] = Unvisited.GetPriority(Unvisited.First);

            //take it off the top, let's see what we can do with it.
            //the int refers to the id of the node we dequeue
            int lastChosen = Unvisited.Dequeue();


            //now that we have it, what are its neighbors?
            //we need to update the previous and distances to all of these objects
            foreach (GraphEdge e in graph.edges[lastChosen])
            {

                // I need to refrain from looking at nodes already visited, otherwise ignore per the foreach
                if (Unvisited.Contains(e.to))
                {
                    // put in words: if the cost of the edge + the distance from the source to the "from" of the current edge < the previously known cost to the "to" of the edge.
                    if (dist[lastChosen] + e.cost < Unvisited.GetPriority(e.to))
                    {
                        // if we've made it into this loop, then we've found an improvement for the path to this node

                        //first, update what is pointing at the current node
                        previous[e.to] = e.from;
                        //update the priority with the new calculated cost
                        Unvisited.UpdatePriority(e.to, dist[lastChosen] + e.cost);
                    }
                }
                
            }
        }

        //TODO : if unvisited.count == 0...return;///////

        //else
        //we are done and have the data...

    }

    // I need to verify in what order the path is delivered.
    public List<int> GetPath()
    {
        List<int> p = new List<int>();
        p.Add(target);
        int next = target;
        while (next!=source)
        {
            p.Add(previous[next]);
            next = previous[next];
        }
        if (p.Count>0)
        {
            p.Reverse();
            return p;
        }
        else
        {
            Debug.Log("Path is a no-go");
            return null;
        }
    }
}