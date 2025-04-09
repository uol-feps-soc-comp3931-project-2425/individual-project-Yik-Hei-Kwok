using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        bool texture_filled = state.new_cube.textures_added.All(s => s != null);
        if (texture_filled)
            state.previewButton.interactable = true;
        else
            state.previewButton.interactable = false;

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
