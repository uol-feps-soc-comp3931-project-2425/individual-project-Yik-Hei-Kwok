using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Place_Cube : CubeClass
{
    public Material placeNewBlock(Mesh meshCube, int blockCount)
    {
        Material mat = base.createCube(false, meshCube, blockCount);

        return mat;
    }
}
