using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State<T> {

    

    // this gets called before Execute ever does
	public virtual void Enter(T entityAI)
    {

    }
    // this gets called every Update cycle
    public abstract void Execute(T entityAI);

    // this gets called before the next state's Enter() command ever does
    public virtual void Exit(T entityAI)
    {

    }
}