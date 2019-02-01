using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// Needed some extra functionality for mid-air jumps to be
/// filtered out if they're not in the entity's current path.
/// </summary>
public class DoubleJumpNode : ActionNode
{
    public override void Awake()
    {
        base.Awake();
        nodeType = GameNodeTypes.DoubleJump;
    }
}
