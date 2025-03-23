using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PC_Generate_Terrain : MonoBehaviour
{
    public Button createTerrain;
    public TMP_InputField inputField_X;
    public TMP_InputField inputField_Y;

    public CreateNewTerrain terrainFunction;
    // Start is called before the first frame update
    void Start()
    {
        //createTerrain.onClick.AddListener(delegate { terrainFunction.initializeTerrain(Int32.Parse(inputField_X.text), Int32.Parse(inputField_X.text)); });
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
