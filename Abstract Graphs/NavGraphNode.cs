using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// Adapted from Mat Buckland's C++ Graph class, in the book "Programming Game AI by Example"
/// </summary>
public class NavGraphNode<T> : GraphNode
{
    //I expect for T to be a dictionary of player IDs, and messages.
    public T extra_info;

    public string id;

    public NavGraphNode(T extra_info, string _id) : base()
    {
        this.extra_info = extra_info;
        id = _id;
    }

    //GameObject lookup method using id(){}


}
