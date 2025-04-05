using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class previewCube : CubeClass
{
    public GameObject cubeView;
    public Merge_Textures mergeTextures;
    public createNewCube createAtlas;
    
    // for allowing user to view how the cube looks like, and confirm if that is what they want
    public void viewCubeResult(bool isTerrain)
    {
        int sizeFinalImage = createAtlas.sizeFinalImage;
       
        // all pixels will end up being in this array (3 rows, row 1 is empty, row 2 and 3 each have three faces)
        mergeTextures.createNewAtlas(sizeFinalImage, isTerrain);

        // modify the texture UV mapping
        Mesh meshCube = GameObject.Find("preview_cube_render").GetComponentInChildren<MeshFilter>().mesh;

        Material material = base.createCube(isTerrain, meshCube, global.blockCount);

        cubeView.GetComponent<Renderer>().material = material;

    }

    

    
}
