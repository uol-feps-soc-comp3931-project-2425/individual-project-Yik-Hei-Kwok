using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clear_Textures : MonoBehaviour
{

    public void clearDisplays(GameObject Screen)
    {
        foreach (Transform t in Screen.transform)
        {
            foreach (Transform small_t in t)
            {
                RawImage texture = small_t.gameObject.GetComponent<RawImage>();
                if (texture != null)
                    texture.texture = null;
                
            }
            
        }
    }
}
