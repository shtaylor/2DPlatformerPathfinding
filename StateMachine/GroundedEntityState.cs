using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  Author: Shane Taylor
///  Expanding on the generic State class with a little more
///  options for the state instances.
/// </summary>
public class GroundedEntityState : State<GroundedEntityAI>
{
    
    public bool direction;
    public Vector2 directionalVector;
    public bool holdingSprint;
    public float timer = -1;
    public float duration = -1;
    
    public override void Execute(GroundedEntityAI entityAI)
    {
        
    }

    public void SetDirectionalVector()
    {
        directionalVector = (direction) ? new Vector2(1, 0) : new Vector2(-1, 0);
    }
}