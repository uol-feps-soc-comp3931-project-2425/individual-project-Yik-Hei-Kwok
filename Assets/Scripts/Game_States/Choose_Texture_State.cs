using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choose_Texture_State : Base_State
{
    public override void EnterState(State_Manager state)
    {
        state.switchScreens(state.Screens, 1);
        state.new_cube.initializeCubeMenu(state.isTerrain, state.cancelPressed);
        state.cancelPressed = false;
    }


    public override void UpdateState(State_Manager state)
    {
        if(global.current_state == State_List.States.cube_preview)
        {
            state.switchState(state.Cube_Preview_State);
        }

        if (global.current_state == State_List.States.start_menu)
        {
            state.switchState(state.start_menu_state);
        }
    }

}
