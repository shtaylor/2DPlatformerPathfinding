using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    // The type of Entity that will be using this state machine
    T owner;
    // current state will execute with Update() in the T script
    public State<T> currentState;
    // for switching back and forth
    // perhaps better would be a chain of states?
    public State<T> previousState;
    // Some global state stuff might be important, such as isDead, isUnspawned, etc...
    public State<T> globalState;

    public StateMachine(T en)
    {
        owner = en;
    }

    // perhaps I should come up with a better way of doing this...
    // as it currently stands, the T that owns this StateMachine will have to run
    // this state machine's Update function within their own Update function
    public void Update()
    {
        if (globalState != null)
        {
            // we could probably use the global state as a way to change states...
            globalState.Execute(owner);
        }
        if (currentState != null)
        {
            currentState.Execute(owner);
        }
        
    }

    // self explanatory, a way of updating references
    // I could just set these to public and not require methods to update...
    public void SetCurrentState(State<T> state) { currentState = state; }
    public void SetPreviousState(State<T> state) { previousState = state; }
    public void SetGlobalState(State<T> state) { globalState = state; }

    public void ChangeState(State<T> newState)
    {

        if (newState == null)
        {
            currentState = null;
            return;
        }

        if (currentState != null)
        {
            //// no reason to go through all this rigamarol if the new state is the same as the current state
            //if (currentState.GetType() == newState.GetType())
            //{
            //    return;
            //}
            //save the previous state
            previousState = currentState;
            // run the current State's exit script
            currentState.Exit(owner);
        }

        

        // set current state to new state
        currentState = newState;
        // run opening command of entering state
        currentState.Enter(owner);
    }

    public void RevertToPreviousState()
    {
        ChangeState(previousState);
    }

    // returns whether the current state is same as the state in question
    public bool IsInState(State<T> newState)
    {
        return (currentState.GetType() == newState.GetType());
    }


}