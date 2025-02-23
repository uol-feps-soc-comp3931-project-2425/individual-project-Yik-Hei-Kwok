using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PC_Choose_Textures : MonoBehaviour
{
    public Button topButton, bottomButton, Side1Button, Side2Button, Side3Button, Side4Button;
    public New_Mesh newMesh;
    // Start is called before the first frame update
    void Start()
    {
        topButton.onClick.AddListener(delegate { newMesh.loadImage("top"); });
        bottomButton.onClick.AddListener(delegate { newMesh.loadImage("bottom"); });
        Side1Button.onClick.AddListener(delegate { newMesh.loadImage("side1"); });
        Side2Button.onClick.AddListener(delegate { newMesh.loadImage("side2"); });
        Side3Button.onClick.AddListener(delegate { newMesh.loadImage("side3"); });
        Side4Button.onClick.AddListener(delegate { newMesh.loadImage("side4"); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
