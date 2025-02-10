using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createBlock : MonoBehaviour
{
    public int scaleCube = 1;

    private Bounds cubeBounds;
    void Start()
    {
        createNewBlock(scaleCube);
    }
    

    private void createNewBlock(int cubeSize)
    {
        GameObject cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeInstance.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

        // get size of the cube in world space
        Renderer cubeRen = cubeInstance.GetComponent<Renderer>();
    }

    
    

}
