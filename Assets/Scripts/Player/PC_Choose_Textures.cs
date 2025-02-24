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
        
        topButton.onClick.AddListener(delegate { newMesh.Apply("Top");  });
        bottomButton.onClick.AddListener(delegate { newMesh.Apply("Bottom"); });
        Side1Button.onClick.AddListener(delegate { newMesh.Apply("Side1");  });
        Side2Button.onClick.AddListener(delegate { newMesh.Apply("Side2");  });
        Side3Button.onClick.AddListener(delegate { newMesh.Apply("Side3");  });
        Side4Button.onClick.AddListener(delegate { newMesh.Apply("Side4");  });
    }

    
}
