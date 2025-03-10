using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_Menu_State : Base_State
{
    

    public override void EnterState(State_Manager state)
    {
        state.switchScreens(state.Screens, 0);
    }


    public override void UpdateState(State_Manager state)
    {

    }
}
