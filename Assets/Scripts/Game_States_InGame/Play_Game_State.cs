using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Play_Game_State : Base_State_InGame
{
    public override void EnterState(State_Manager_InGame state)
    {
        state.switchScreens(state.Screens, 0);
        global.current_state = State_List.States.playing_game;
        // cannot reset texture of terrain for now
        state.isTerrain = false;
        // update inventory
        UpdateInventory(state);
    }


    private void UpdateInventory(State_Manager_InGame state)
    {
        for (int i = 1; i < state.inventory.transform.childCount + 1; i++)
        {
            Transform childInventory = state.inventory.transform.Find($"Slot{i}");
            Transform blockF = childInventory.Find("block_f");
            // for each cube, check if there is an atlas in the corresponding folder
            foreach (Transform t in blockF)
            {
                // if an atlaas exist for the  block, show the block with the atlas texture
                bool exists = System.IO.File.Exists($"Assets/Saved/Final_Image/{i-1}/atlas.png");
                
                if (exists == true)
                {
                    Debug.Log("t.gameObject = " + t.gameObject);
                    t.gameObject.SetActive(true);
                    Mesh meshCube = t.gameObject.GetComponent<MeshFilter>().mesh;

                    Material material = state.cubeClass.createCube(false, meshCube, i-1);

                    t.gameObject.GetComponent<Renderer>().material = material;
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
                    
                    
            }
        }
        
    }


    public override void UpdateState(State_Manager_InGame state)
    {
        
    }
}
