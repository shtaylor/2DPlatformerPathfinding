using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// This is the general Node class that has the necessary
/// functions for setting its neighbors, the edge weights,
/// the applicable messages, and so on.
/// </summary>
public class Node : MonoBehaviour {

    // This gets set by GameGraph
    public int ID;

    public List<Node> neighbors = new List<Node>();
    public List<Message> messages = new List<Message>();
    public List<float> weights;


    public GameNodeTypes nodeType;

    // this should be updated by derived class
    public virtual void SetWeights()
    {
        weights = new List<float>();
        //for (int i = 0; i < messages.Count; i++)
        //{
        //    weights[i] = 0;
        //}
    }



    public virtual void Awake()
    {
        Destroy(GetComponent<MeshRenderer>());
        nodeType = GameNodeTypes.None;
        Collider2D c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    public void ResetBoxCollider()
    {
        BoxCollider2D col = this.GetComponent<BoxCollider2D>();
        Vector2 size = col.size;
        GameObject.Destroy(col);
        col = this.gameObject.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = true;
    }



    public Node GetLeftNeighbor()
    {
        int i = -1;
        i = messages.IndexOf(Message.WalkLeft);
        if (i != -1)
        {
            return neighbors[i];
        }
        else return null;
    }

    public Node GetRightNeighbor()
    {
        int i = -1;
        i = messages.IndexOf(Message.WalkRight);
        if (i != -1)
        {
            return neighbors[i];
        }
        else return null;
    }

    public void SetLeftNeighbor(Node newNode)
    {
        int i = -1;
        i = messages.IndexOf(Message.WalkLeft);
        if (i == -1)
        {
            neighbors.Add(newNode);
            messages.Add(Message.WalkLeft);
        }
        else
        {
            neighbors[i] = newNode;
        }
    }

    public void SetRightNeighbor(Node newNode)
    {
        int i = -1;
        i = messages.IndexOf(Message.WalkRight);
        if (i == -1)
        {
            neighbors.Add(newNode);
            messages.Add(Message.WalkRight);
        }
        else
        {
            neighbors[i] = newNode;
        }
    }

}
