using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choose_Texture_State : Base_State
{
    public override void EnterState(State_Manager state)
    {
        state.switchScreens(state.Screens, 1);
    }


    public override void UpdateState(State_Manager state)
    {


        if(global.current_state == State_List.States.cube_preview)
        {
            state.switchState(state.Cube_Preview_State);
        }
    }

}
