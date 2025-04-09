using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;
public class Choose_Texture_State_InGame : Base_State_InGame
{
    public override void EnterState(State_Manager_InGame state)
    {
        state.lockCursor(false);
        state.controlScript.inMenu = true;
        state.backToGame.onClick.AddListener(delegate { returnToGame(state); });
        state.switchScreens(state.Screens, 1);

        state.new_cube.initializeCubeMenu(state.isTerrain, state.cancelPressed);
        state.cancelPressed = false;
    }


    public override void UpdateState(State_Manager_InGame state)
    {
        bool texture_filled = state.new_cube.textures_added.All(s => s != null);
        if (texture_filled)
            state.previewButton.interactable = true;
        else
            state.previewButton.interactable = false;

        if (global.current_state == State_List.States.cube_preview)
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
