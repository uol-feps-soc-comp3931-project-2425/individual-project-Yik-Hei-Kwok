using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PC_ChooseSize : MonoBehaviour
{
    public Button confirmButtonSettings; 
    public Button confirmButtonSize;
    public createNewCube newMesh;
    // Start is called before the first frame update
    void Start()
    {
        confirmButtonSettings.onClick.AddListener(delegate { newMesh.settingsConfirm(); });
        confirmButtonSize.onClick.AddListener(delegate { newMesh.sizeConfirm(); });
        //discardButton.onClick.AddListener(delegate { newMesh.outputSizeCancel(); });

    }
}
