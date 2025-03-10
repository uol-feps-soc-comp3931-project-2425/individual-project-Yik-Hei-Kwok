using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Controls : MonoBehaviour
{
    public PlayerControls playerControls;
    private InputAction RightMouse;
    private InputAction leftMouse;
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        leftMouse = playerControls.Controls.LeftMouse;
        RightMouse = playerControls.Controls.RightMouse;

        leftMouse.Enable();
        RightMouse.Enable();

        // detect when mouse is pressed
        leftMouse.performed += leftClick;
        RightMouse.performed += RightClick;

        //detect when mouse is released
        leftMouse.canceled += leftClick_Released;
        RightMouse.canceled += rightClick_Released;
    }

    private void OnDisable()
    {
        leftMouse.Disable();
        RightMouse.Disable();

        leftMouse.performed -= leftClick;
        RightMouse.performed -= RightClick;
        leftMouse.canceled -= leftClick_Released;
        RightMouse.canceled -= rightClick_Released;

    }

    // runs when the left/right mouse button is pressed
    private void leftClick(InputAction.CallbackContext context)
    {
        Debug.Log("Left Mouse Pressed");
        global.mouseHoldLeft = true;
    }

    private void RightClick(InputAction.CallbackContext context)
    {
        Debug.Log("Right Mouse Pressed");
        global.mouseHoldRight = true;

        
    }

    // runs when the left/right mouse button is released
    private void leftClick_Released(InputAction.CallbackContext context)
    {
        global.mouseHoldLeft = false;
    }

    private void rightClick_Released(InputAction.CallbackContext context)
    {
        global.mouseHoldRight = false;

        // when previewing cube
    }



}
