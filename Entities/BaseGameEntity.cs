using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Shane Taylor
/// BaseGameEntity just has an option for an ID. Other classes that derive from
/// this one will have to set their unique ID's upon instantiation.
/// </summary>

[RequireComponent(typeof(Controller2D))]
public class BaseGameEntity : MonoBehaviour {

    

    // I'm thinking that I'm going to go with a string for the id, despite the difficulties of
    // working with strings in general.
    public string ID { get; set; }

    protected virtual void Awake()
    {
        ID = "HasNoDerivedClass";
    }
}