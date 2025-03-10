using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cube_Preview_State : Base_State
{
    private Vector2 currentMousePosition;
    private Vector2 prevMousePosition;
    private float cubeRotationSpeed = 0.5f;
    public override void EnterState(State_Manager state)
    {
        state.switchScreens(state.Screens, 2);
        //disable all stuff in the first screen
        state.enableOrDisableChildren("Screen_1", false);
        // enable all stuff in the second screen
        state.enableOrDisableChildren("Screen_2", true);
        // show the preview cube
        state.prevCubeFunc.viewCubeResult();
    }


    public override void UpdateState(State_Manager state)
    {
        // Update the current mouse position
        currentMousePosition = Mouse.current.position.ReadValue();

        if (global.mouseHoldRight)
        {
            // Calculate the change in mouse position
            Vector2 change = currentMousePosition - prevMousePosition;
            Debug.Log("change = " + change);

            float rotation_x = -change.y * cubeRotationSpeed * Time.deltaTime;
            float rotation_y = -change.x * cubeRotationSpeed * Time.deltaTime;
            // Perform cube rotation based on mouse movement
            state.previewCubeObject.transform.Rotate(rotation_x, rotation_y, 0 , Space.World);

           
        }
        else
        {
            // Update the previous mouse position when the right mouse button is not held
            prevMousePosition = currentMousePosition;
        }
    }
}
