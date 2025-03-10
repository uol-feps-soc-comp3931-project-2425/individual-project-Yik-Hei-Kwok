using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base_State
{
    public abstract void EnterState(State_Manager state);

    public abstract void UpdateState(State_Manager state);


}
