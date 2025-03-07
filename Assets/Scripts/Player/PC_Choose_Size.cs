using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PC_ChooseSize : MonoBehaviour
{
    public Button confirmButton; //, discardButton;
    public createNewCube newMesh;
    // Start is called before the first frame update
    void Start()
    {
        confirmButton.onClick.AddListener(delegate { newMesh.outputSizeConfirm(); });
        //discardButton.onClick.AddListener(delegate { newMesh.outputSizeCancel(); });
        
    }
}
