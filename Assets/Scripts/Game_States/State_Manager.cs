using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class State_Manager : MonoBehaviour
{
    // to make sure state manager doesn't get destroyed on load
    private static State_Manager instance;

    public Base_State current_state;

    public Start_Menu_State start_menu_state = new Start_Menu_State();
    public Choose_Texture_State choose_texture_state = new Choose_Texture_State();
    public Cube_Preview_State Cube_Preview_State = new Cube_Preview_State();
    

    // Activate canvases
    public GameObject Screens;

    // Activate button Scripts
    public PC_ChooseSize PC_ChooseSize;
    public PC_Choose_Textures PC_ChooseTextures;

    // for screen 0 (start screen)
    public createNewCube new_cube;
    public CreateNewTerrain Create_Terrain;
    public TMP_InputField input_x_terrain;
    public TMP_InputField input_y_terrain;
    public Button chooseTexture;
    public Button createTerrain;

    // for screen  1
    public bool isTerrain = false;
    public Button previewButton;

    // for screen 2
    public previewCube prevCubeFunc;
    public GameObject previewCubeObject;
    public Button texture_confirmButton;
    public Button texture_cancelButton;
    public bool cancelPressed = false;


    public Clear_Textures clearTextures;

    private void Start()
    {

        current_state = start_menu_state;

        current_state.EnterState(this);
    }

    private void Update()
    {
        current_state.UpdateState(this);
    }

    public void switchState(Base_State state)
    {
        current_state = state;
        state.EnterState(this);
    }

    public Base_State GetCurrentState()
    {
        return current_state;
    }

    // switching between screens
    public void switchScreens(GameObject screens, int index)
    {
        foreach (Transform t in screens.transform)
        {
            t.gameObject.SetActive(false);
        }
        screens.transform.GetChild(index).gameObject.SetActive(true);
    }

    // for enabling and disabling children objects
    public void enableOrDisableChildren(string objectName, bool active)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            foreach (Transform t in obj.transform)
                t.gameObject.SetActive(active);
        }
    }



}
