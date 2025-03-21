using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controls_Building : MonoBehaviour
{
    public TextureSynthesisGame playerControls;
    public PlayerControls playerControls2;
    private InputAction RightMouse, leftMouse;
    private InputAction WASD;
    private InputAction Up, Down;
    private InputAction PositionMouse;

    private Camera cameraMain;
    private Rigidbody rb;

    public float cameraLookSpeed = 0.1f;

    private void Awake()
    {

        // initialize the controls
        playerControls = new TextureSynthesisGame();
        WASD = playerControls.Player_Controls.Move_4_axis;
        PositionMouse = playerControls.Player_Controls.Mouse_Position;

        // get the player camera
        cameraMain = gameObject.transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        // get rigidbody of player
        rb = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {
        WASD.Enable();
        PositionMouse.Enable();
    }

    private void OnDisable()
    {
        WASD.Disable();
        PositionMouse.Disable();
    }






    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        RotateCamera();
       



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


}
