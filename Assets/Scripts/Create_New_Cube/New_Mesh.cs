using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class New_Mesh : MonoBehaviour
{
    public void createNewTextureMesh()
    {
        // create new cube for display
        GameObject cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);


        // since Unity by default only supports the same texture on each side of cube,
        // need to make sure we set the UV mapping for customization on each side
        setUVMapping();
    }

    private void setUVMapping()
    {

    }
}
