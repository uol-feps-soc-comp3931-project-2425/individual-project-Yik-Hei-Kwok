using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player_Controls_Building : MonoBehaviour
{
    public TextureSynthesisGame playerControls;
    public PlayerControls playerControls2;
    private InputAction RightMouse, LeftMouse;
    private InputAction WASD;
    private InputAction Up, Down;
    private InputAction PositionMouse;
    private InputAction InventoryChoose;
    private InputAction CreateNewBlock;

    private Camera cameraMain;
    private Rigidbody rb;


    public LayerMask detectionLayer;

    public float cameraLookSpeed = 0.1f;

    public int inventorySize = 9;
    public GameObject inventory;

    

    private void Awake()
    {

        // initialize the controls
        playerControls = new TextureSynthesisGame();
        WASD = playerControls.Player_Controls.Move_4_axis;
        PositionMouse = playerControls.Player_Controls.Mouse_Position;
        RightMouse = playerControls.Player_Controls.Right_Mouse;
        LeftMouse = playerControls.Player_Controls.Left_Mouse;

        InventoryChoose = playerControls.Player_Controls.Inventory_Choose;

        CreateNewBlock = playerControls.Player_Controls.New_Block_Create;

        // get the player camera
        cameraMain = gameObject.transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        // get rigidbody of player
        rb = GetComponent<Rigidbody>();

        // set to default inventory slot
        changeInventory(1);
    }


    private void OnEnable()
    {
        WASD.Enable();
        PositionMouse.Enable();
        InventoryChoose.Enable();
        CreateNewBlock.Enable();
        RightMouse.Enable();
        LeftMouse.Enable();

        InventoryChoose.performed += changeInventory;
        CreateNewBlock.performed += invokeCreateMenu;

        RightMouse.performed += RightClicked;
        LeftMouse.performed += LeftClicked;
        RightMouse.canceled += RightReleased;
        LeftMouse.canceled += LeftReleased;

    }

    

    private void OnDisable()
    {
        InventoryChoose.performed -= changeInventory;
        CreateNewBlock.performed -= invokeCreateMenu;

        RightMouse.performed -= RightClicked;
        LeftMouse.performed -= LeftClicked;
        RightMouse.canceled -= RightReleased;
        LeftMouse.canceled -= LeftReleased;


        WASD.Disable();
        PositionMouse.Disable();
        InventoryChoose.Disable();
        CreateNewBlock.Disable();
        RightMouse.Disable();
        LeftMouse.Disable();


    }






    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        RotateCamera();

        // create raycast from center of camera to detect closest block
        RayCast();

    }

    private void RayCast()
    {
        RaycastHit hit;
        var ray = Physics.Raycast(cameraMain.transform.position, cameraMain.transform.forward, out hit, 100.0f, detectionLayer);
        if (ray)
        {

            Debug.Log("Interacted Object = " + hit.transform.gameObject);
        }
    }

    private void MoveCamera()
    {
        // for using WASD controls to move front back left right
        Vector2 moveInput = WASD.ReadValue<Vector2>();
        float moveX = moveInput.x;
        float moveY = moveInput.y;

        // set the direction 
        Vector3 direction = moveY * cameraMain.transform.forward + moveX * cameraMain.transform.right;

        gameObject.transform.position += direction * 2f * Time.deltaTime;
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision.gameObject.layer = " + collision.gameObject.layer);
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("Touched");
        }
    }


    private void RotateCamera()
    {
        Vector2 mousePosition = PositionMouse.ReadValue<Vector2>();

        float directionX = mousePosition.x - (Screen.width / 2);
        float directionY = mousePosition.y - (Screen.height / 2);

        cameraMain.transform.localRotation = Quaternion.Euler(-directionY * cameraLookSpeed, directionX * cameraLookSpeed, 0);
    }

    // changing inventory
    private void changeInventory(InputAction.CallbackContext context)
    {
        // check which button is being pressed
        InputControl control = context.control;
        int bindingIndex = InventoryChoose.GetBindingIndexForControl(control);

        changeInventory(bindingIndex);
    }
    private void changeInventory(int bindingIndex)
    {
        for (int i = 1; i < 10; i++)
        {
            GameObject child_inv = inventory.transform.Find($"Slot{i}").gameObject;
            Image border = child_inv.GetComponent<Image>();

            if (i == bindingIndex + 1)
            {
                border.color = Color.cyan;
            }
            else
            {
                border.color = Color.white;
            }
        }
    }

    // for invoking the creation of a new block
    private void invokeCreateMenu(InputAction.CallbackContext context)
    {
        // create new gameobject for storing terrain blocks
        global.current_state = State_List.States.choose_textures;
        State_Manager_InGame state_manager = GameObject.Find("State_Manager").GetComponent<State_Manager_InGame>();
        state_manager.switchState(state_manager.choose_texture_state);
        
    }

    // for detecting clicks
    private void RightClicked(InputAction.CallbackContext context)
    {
        global.mouseHoldRight = true;
    }

    private void LeftClicked(InputAction.CallbackContext context)
    {
        global.mouseHoldLeft = true;
    }

    private void RightReleased(InputAction.CallbackContext context)
    {
        global.mouseHoldRight = false;
    }

    private void LeftReleased(InputAction.CallbackContext context)
    {
        global.mouseHoldLeft = false;
    }



}
