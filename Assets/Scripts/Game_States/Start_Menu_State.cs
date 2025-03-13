using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Start_Menu_State : Base_State
{
    private int x_dim;
    private int y_dim;

    private bool invalidX = false;
    private bool invalidY = true;

    private Button generateButton;
    private TextMeshProUGUI warningText;

    public override void EnterState(State_Manager state)
    {
        state.switchScreens(state.Screens, 0);

        // listen to input of x and y dimensions of terrain
        state.input_x_terrain.onEndEdit.AddListener(Terrain_X_Dimensions);
        state.input_y_terrain.onEndEdit.AddListener(Terrain_Y_Dimensions);

        generateButton = state.Screens.transform.Find("Screen_0/Terrain_Button").gameObject.GetComponent<Button>();

        warningText = state.Screens.transform.Find("Screen_0/Warning").gameObject.GetComponent<TextMeshProUGUI>();
    }


    public override void UpdateState(State_Manager state)
    {
        // if one of the input values is not acceptable, show warning, and set the button to false
        if (invalidX || invalidY )
        {
            warningText.text = "Dimensions not allowed";

            generateButton.interactable = false;
        }
        // only activate the generate terrain button if input values are acceptable
        if (!invalidX &&  !invalidY)
        {
            warningText.text = "";

            generateButton.interactable = true;
        }
    }
    

    private void Terrain_X_Dimensions(string arg0)
    {
        int num_x;
        bool canConvert = Int32.TryParse(arg0, out num_x);
        if (canConvert == false || num_x < 15 || num_x > 100)
        {
            invalidX = true;

        }
        else
        {
            invalidX = false;
        }
    }

    private void Terrain_Y_Dimensions(string arg0)
    {
        int num_y;
        bool canConvert = Int32.TryParse(arg0, out num_y);
        if (canConvert == false || num_y < 15 || num_y > 100)
        {
            invalidY = true;

        }
        else
        {
            invalidY = false;
        }
    }
}
