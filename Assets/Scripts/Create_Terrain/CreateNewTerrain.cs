using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewTerrain : MonoBehaviour
{
    public void initializeTerrain(int x, int z)
    {
        // x and y refers to the number of blocks
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(x, 0, z);
    }
}
