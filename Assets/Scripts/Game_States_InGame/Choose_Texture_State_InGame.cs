using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Choose_Texture_State_InGame : Base_State_InGame
{
    public override void EnterState(State_Manager_InGame state)
    {
        state.controlScript.inMenu = true;
        state.backToGame.onClick.AddListener(delegate { returnToGame(state); });
        state.switchScreens(state.Screens, 1);
        state.new_cube.initializeCubeMenu(state.isTerrain, state.cancelPressed);
        state.cancelPressed = false;
    }


    public override void UpdateState(State_Manager_InGame state)
    {
        if(global.current_state == State_List.States.cube_preview)
        {
            state.switchState(state.Cube_Preview_State);
        }
    }

    private void returnToGame(State_Manager_InGame state)
    {
        global.current_state = State_List.States.playing_game;
        // turn back on directional light
        GameObject.Find("Directional Light").GetComponent<Light>().enabled = true;
        state.switchState(state.Play_Game_State);

    }

}
