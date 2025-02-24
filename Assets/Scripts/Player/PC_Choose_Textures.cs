using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PC_Choose_Textures : MonoBehaviour
{
    public Button topButton, bottomButton, Side1Button, Side2Button, Side3Button, Side4Button;
    public New_Mesh newMesh;

    // indentify which button is pressed (Top 0 / Bottom 1 / Side1 2 / Side2 3 / Side3 4/ Side4 5)
    public int processing_side;
    // Start is called before the first frame update
    void Start()
    {
        
        topButton.onClick.AddListener(delegate { newMesh.loadImage("top"); processing_side = 0; });
        bottomButton.onClick.AddListener(delegate { newMesh.loadImage("bottom"); processing_side = 1; });
        Side1Button.onClick.AddListener(delegate { newMesh.loadImage("side1"); processing_side = 2; });
        Side2Button.onClick.AddListener(delegate { newMesh.loadImage("side2"); processing_side = 3; });
        Side3Button.onClick.AddListener(delegate { newMesh.loadImage("side3"); processing_side = 4; });
        Side4Button.onClick.AddListener(delegate { newMesh.loadImage("side4"); processing_side = 5; });
    }

    
}
