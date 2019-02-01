using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// ActionNodes are the nodes that have messages for actions such as jumping,
/// dodging, etc... They also have simple walkleft and walkright messages to
/// deal connect them to adjacent floornodes.
/// 
/// Note: Action nodes are all that is required to implement wall climbing,
/// which was surprising to me at first. Quite a nice surprise.
/// </summary>
public class ActionNode : Node {

    List<Message> shortJumps;
    List<Message> mediumJumps;

    public override void Awake()
    {
        base.Awake();
        shortJumps = new List<Message>() { Message.JumpLeft1, Message.JumpLeft2, Message.JumpRight1, Message.JumpRight2 };
        mediumJumps = new List<Message>() { Message.JumpLeft3, Message.JumpRight3,Message.Jump3 };

        nodeType = GameNodeTypes.ActionNode;
        //longJumps = new List<Message>() { Message.JumpLeft5, Message.JumpRight5, Message.DodgeLeft, Message.DodgeRight, Message.Jump5 };
    }

    public override void SetWeights()
    {
        base.SetWeights();

        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i] == Message.WalkLeft || messages[i] == Message.WalkRight)
            {
                weights.Add(.1f);
            }
            else if (shortJumps.Contains(messages[i]))
            {
                weights.Add(1);
            }
            else if (mediumJumps.Contains(messages[i]))
            {
                weights.Add(1.5f);
            }
            else
            {
                weights.Add(2);
            }
        }

        // This is how I did it in the old version
        //for (int i = 0; i < messages.Count; i++)
        //{
        //    if (messages[i] == Message.JumpLeft1 || messages[i] == Message.JumpRight1 || messages[i] == Message.JumpLeftThenRight1 || messages[i] == Message.JumpRightThenLeft1 || messages[i] == Message.RunningJumpLeft1 || messages[i] == Message.RunningJumpRight1)
        //    {
        //        weights.Add(1.5f * EntityConstants.MINJUMPDURATION);
        //    }
        //    else if (messages[i] == Message.JumpLeft2 || messages[i] == Message.JumpRight2 || messages[i] == Message.JumpLeftThenRight2 || messages[i] == Message.JumpRightThenLeft2 || messages[i] == Message.RunningJumpLeft2 || messages[i] == Message.RunningJumpRight2)
        //    {
        //        weights.Add(1.5f * EntityConstants.MIDJUMPDURATION);
        //    }
        //    else if (messages[i] == Message.JumpLeft3 || messages[i] == Message.JumpRight3 || messages[i] == Message.JumpLeftThenRight3 || messages[i] == Message.JumpRightThenLeft3 || messages[i] == Message.RunningJumpLeft3 || messages[i] == Message.RunningJumpRight3)
        //    {
        //        weights.Add(1.5f * EntityConstants.MAXJUMPDURATION);
        //    }
        //    else
        //    {
        //        weights.Add(0);
        //    }
        //}
    }
}
