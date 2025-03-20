using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class previewCube : MonoBehaviour
{
    public GameObject cubeView;
    public Merge_Textures mergeTextures;
    public CreateNewTerrain createTerrain;

    
    // for allowing user to view how the cube looks like, and confirm if that is what they want
    public void viewCubeResult(bool isTerrain)
    {
        int sizeFinalImage = FindObjectOfType<createNewCube>().sizeFinalImage;
        

        // all pixels will end up being in this array (3 rows, row 1 is empty, row 2 and 3 each have three faces)
        mergeTextures.createNewAtlas(sizeFinalImage, isTerrain);

        // modify the texture UV mapping
        Mesh meshCube = GameObject.Find("Screen_2").GetComponentInChildren<MeshFilter>().mesh;

        Vector2[] UVs = setUVMapping(meshCube);

        meshCube.uv = UVs;
        string path;
        if (isTerrain)
        {
            path = "Terrain";
            
        }
        else
        {
            path = $"{global.blockCount}";
        }

        var createdTexture = File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/atlas.png");
        Texture2D newTexture = new Texture2D(2, 2);
        newTexture.LoadImage(createdTexture);

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = newTexture;

        //AssetDatabase.CreateAsset(material, "Assets/Saved/Final_Image/0/atlas.png");

        //AssetDatabase.SaveAssets();
        cubeView.GetComponent<Renderer>().material = material;

    }

    private void confirmTerrainTexture()
    {
        createTerrain.terrainTextureSelected = true;
    }


    private Vector2[] setUVMapping(Mesh mesh)
    {
        Vector2[] UVs = new Vector2[mesh.vertices.Length];
        // Front
        UVs[0] = new Vector2(0.0f, 0.0f);
        UVs[1] = new Vector2(0.333f, 0.0f);
        UVs[2] = new Vector2(0.0f, 0.333f);
        UVs[3] = new Vector2(0.333f, 0.333f);

        // Top
        UVs[4] = new Vector2(0.334f, 0.333f);
        UVs[5] = new Vector2(0.666f, 0.333f);
        UVs[8] = new Vector2(0.334f, 0.0f);
        UVs[9] = new Vector2(0.666f, 0.0f);

        // Back
        UVs[6] = new Vector2(1.0f, 0.0f);
        UVs[7] = new Vector2(0.667f, 0.0f);
        UVs[10] = new Vector2(1.0f, 0.333f);
        UVs[11] = new Vector2(0.667f, 0.333f);

        // Bottom
        UVs[12] = new Vector2(0.0f, 0.334f);
        UVs[13] = new Vector2(0.0f, 0.666f);
        UVs[14] = new Vector2(0.333f, 0.666f);
        UVs[15] = new Vector2(0.333f, 0.334f);

        // Left
        UVs[16] = new Vector2(0.334f, 0.334f);
        UVs[17] = new Vector2(0.334f, 0.666f);
        UVs[18] = new Vector2(0.666f, 0.666f);
        UVs[19] = new Vector2(0.666f, 0.334f);

        // Right        
        UVs[20] = new Vector2(0.667f, 0.334f);
        UVs[21] = new Vector2(0.667f, 0.666f);
        UVs[22] = new Vector2(1.0f, 0.666f);
        UVs[23] = new Vector2(1.0f, 0.334f);

        return UVs;
    }
}
