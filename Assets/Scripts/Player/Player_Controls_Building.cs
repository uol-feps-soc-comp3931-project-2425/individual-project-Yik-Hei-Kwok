using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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
    private InputAction Jump;

    private Camera cameraMain;
    private Rigidbody rb;
    private int bindingIndex;

    private int space_counter = 0;
    private int flight_mode = 0;
    private bool startTimerJump = false;
    public float timer = 0.3f;
    private float timer_saved = 0.3f;
    private int spaceHeld = 0;
    private float flyVerticalSpeed = 3f;
    private float flyHorizontalSpeed = 5f;

    private Vector3 positionToBePlaced = Vector3.zero;
    private GameObject pointedBlock;

    public LayerMask detectionLayer;

    private int placeCount = 0;

    public float cameraLookSpeed = 0.1f;

    public int inventorySize = 9;
    public GameObject inventory;
    public bool inMenu;
    public GameObject PreviewPlacement;
    public State_Manager_InGame manager;
    public Place_Cube cubeClassScript;

    public float jumpForce;

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

        Jump = playerControls.Player_Controls.Jump;
        // get the player camera
        cameraMain = gameObject.transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        // get rigidbody of player
        rb = GetComponent<Rigidbody>();

        // set to default inventory slot
        changeInventory(0);

        inMenu = false;

        // freeze the player rotation so that player doesn't fall down
        rb.freezeRotation = true;
    }


    private void OnEnable()
    {
        WASD.Enable();
        PositionMouse.Enable();
        InventoryChoose.Enable();
        CreateNewBlock.Enable();
        RightMouse.Enable();
        LeftMouse.Enable();
        Jump.Enable();

        InventoryChoose.performed += changeInventory;
        CreateNewBlock.performed += invokeCreateMenu;

        Jump.performed += spacePressed;
        Jump.canceled += spaceCancelled;


        RightMouse.performed += RightClicked;
        LeftMouse.performed += LeftClicked;
        RightMouse.canceled += RightReleased;
        LeftMouse.canceled += LeftReleased;

    }

    private void OnDisable()
    {
        InventoryChoose.performed -= changeInventory;
        CreateNewBlock.performed -= invokeCreateMenu;

        Jump.performed -= spacePressed;
        Jump.canceled -= spaceCancelled;

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
        Jump.Disable();


    }

    void Update()
    {
        var wasPressed = Jump.triggered && Jump.ReadValue<float>() > 0;
        var wasReleased = Jump.triggered && Jump.ReadValue<float>() == default;

        if (inMenu == false)
        {
            MoveCamera();
            RotateCamera();

            // create raycast from center of camera to detect closest block
            RayCast();

            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
        }
        int count = 0;
        foreach (int i in manager.inventoryList)
        {
            Debug.Log($"inventory_list {count} = {i}");
            count++;
        }
        

        // for jump timer
        if (startTimerJump == true)
        {
            if (timer > 0)
                timer -= Time.deltaTime;
            else
            {
                // reset the space counter after the time is up
                timer = timer_saved;
                space_counter = 0;
                startTimerJump = false;
            }
                
        }
    }


    private void FixedUpdate()
    {
        // fly upwards
        if (flight_mode == 1 && spaceHeld == 1)
        {
            transform.position += Vector3.up * flyVerticalSpeed * Time.deltaTime;
        }
    
    }

    private void RayCast()
    {
        RaycastHit hit;
        var ray = Physics.Raycast(cameraMain.transform.position, cameraMain.transform.forward, out hit, 100.0f, detectionLayer);
        if (ray)
        {
            pointedBlock = hit.transform.gameObject;
            // get which cube face the ray is detecting
            Vector3 normal = hit.normal;
            Vector3 localForward = hit.transform.forward;
            Vector3 localRight = hit.transform.right;
            Vector3 localUp = hit.transform.up;

            Vector3 direction = Vector3.zero;
            string direction_Debug = "ur mom";
            // get the dot product values
            float dotProductLocForward = Vector3.Dot(normal, localForward);
            float dotProductLocBackward = Vector3.Dot(normal, -localForward);
            float dotProductLocRight = Vector3.Dot(normal, localRight);
            float dotProductLocLeft = Vector3.Dot(normal, -localRight);
            float dotProductLocUp = Vector3.Dot(normal, localUp);
            float dotProductLocDown = Vector3.Dot(normal, -localUp);

            if (dotProductLocForward > 0.9f)
                //direction_Debug = "forward";
                direction = Vector3.forward;
            else if (dotProductLocBackward > 0.9f)
                //direction_Debug = "backward";
                direction = Vector3.back;
            else if (dotProductLocRight > 0.9f)
                //direction_Debug = "rightward";
                direction = Vector3.right;
            else if (dotProductLocLeft > 0.9f)
                //direction_Debug = "leftward";
                direction = Vector3.left;
            else if (dotProductLocUp > 0.9f)
                //direction_Debug = "upward";
                direction = Vector3.up;
            else if (dotProductLocDown > 0.9f)
                //direction_Debug = "downward";
                direction = Vector3.down;

            showPlacementPreview(hit.transform,direction);





            Debug.Log("Interacted Object " + hit.transform.gameObject + " on direction " + direction_Debug);
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

    private void spacePressed(InputAction.CallbackContext context)
    {
        space_counter += 1;
        if (flight_mode == 0)
        {
            rb.useGravity = true;
            // jump
            if (space_counter == 1)
            {
                Debug.Log("Jumped");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                startTimerJump = true;
            }
            // switch to flight mode
            else
            {
                space_counter = 0;
                startTimerJump = false;
                timer = timer_saved;

                flight_mode = 1;
                spaceHeld = 0;
                rb.useGravity = false;

                // stop the jump force
                rb.velocity = Vector3.zero;
            }
        }
        // currently flying
        else if (flight_mode == 1)
        {

            spaceHeld = 1;
            if (space_counter == 1)
                startTimerJump = true;
            // turn back to gravity mode
            else
            {
                space_counter = 0;
                startTimerJump = false;
                timer = timer_saved;

                flight_mode = 0;
                rb.useGravity = true;
            }
                
        }
        
        
    }

    private void spaceCancelled(InputAction.CallbackContext context)
    {
        spaceHeld = 0;
    }



    // changing inventory
    private void changeInventory(InputAction.CallbackContext context)
    {
        // check which button is being pressed
        InputControl control = context.control;
        bindingIndex = InventoryChoose.GetBindingIndexForControl(control);

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


    // show where the cube will be placed
    private void showPlacementPreview(Transform raycastedCube, Vector3 direction)
    {
        // check if the player has block in corresponding inventory.
        // Only show preview if there is one
        if (manager.inventoryList[bindingIndex] == 1)
        {
            GameObject pointedCube = raycastedCube.gameObject;
            showPlacement(direction, pointedCube);
        }
        else
        {
            foreach (Transform child in PreviewPlacement.transform)
            {
                Destroy(child.gameObject);
            }
        }
       
    }

    private void showPlacement(Vector3 direction, GameObject pointedCube)
    {
        // clear all objects in PreviewPlacement 
        foreach (Transform child in PreviewPlacement.transform)
        {
            Destroy(child.gameObject);
        }
        Vector3 positionPointedCube = pointedCube.transform.position;

        positionToBePlaced = positionPointedCube + direction;


        GameObject previewOnTerrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer renderer = previewOnTerrain.GetComponent<Renderer>();
        renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprite/transparency.mat");
        previewOnTerrain.GetComponent<Collider>().enabled = false;

        previewOnTerrain.transform.parent = PreviewPlacement.transform;

        previewOnTerrain.transform.position = positionToBePlaced;
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

        // if there is a block in hand, place the block at the area defined in showPlacement
        if(manager.inventoryList[bindingIndex] == 1)
        {
            GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = newCube.GetComponent<MeshFilter>().mesh;
            Material mat = cubeClassScript.placeNewBlock(mesh, bindingIndex);
            newCube.GetComponent<MeshRenderer>().material = mat;

            newCube.transform.position = positionToBePlaced;
            newCube.name = $"id_{placeCount}";
            newCube.transform.parent = GameObject.Find("Placed_Blocks").transform;
            newCube.layer = LayerMask.NameToLayer("Tile");
            placeCount++;
        }
    }

    private void LeftClicked(InputAction.CallbackContext context)
    {
        // destroy the block being pointed to
        Destroy(pointedBlock);
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
