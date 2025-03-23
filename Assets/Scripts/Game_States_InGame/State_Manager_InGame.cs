using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class State_Manager_InGame : MonoBehaviour
{
    // to make sure state manager doesn't get destroyed on load
  

    public Base_State_InGame current_state;

    public Play_Game_State Play_Game_State = new Play_Game_State();
    public Choose_Texture_State_InGame choose_texture_state = new Choose_Texture_State_InGame();
    public Cube_Preview_State_InGame Cube_Preview_State = new Cube_Preview_State_InGame();

    // Activate canvases
    public GameObject Screens;

    // Activate button Scripts
    public PC_ChooseSize PC_ChooseSize;
    public PC_Choose_Textures PC_ChooseTextures;
    public GameObject inventory;
    public CubeClass cubeClass;

    // for screen  1
    public bool isTerrain = false;
    public createNewCube new_cube;
    public Button backToGame;

    // for screen 2
    public previewCube prevCubeFunc;
    public GameObject previewCubeObject;
    public Button texture_confirmButton;
    public Button texture_cancelButton;
    public bool cancelPressed = false;

    

    private void Start()
    {

        current_state = Play_Game_State;

        current_state.EnterState(this);
    }

    private void Update()
    {
        current_state.UpdateState(this);
    }

    public void switchState(Base_State_InGame state)
    {
        current_state = state;
        state.EnterState(this);
    }

    public Base_State_InGame GetCurrentState()
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
