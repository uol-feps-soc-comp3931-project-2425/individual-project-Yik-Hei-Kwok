using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base_State_InGame
{
    public abstract void EnterState(State_Manager_InGame state);

    public abstract void UpdateState(State_Manager_InGame state);


}
