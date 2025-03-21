using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateNewTerrain : CubeClass
{
    // detect if the select texture button is pressed (should be set back to false after terrain texture is selected)
    public bool terrainTextureSelected = false;
    public void initializeTerrain(int x, int z)
    {
        // load the scene 
        SceneManager.LoadScene(sceneName: "New_Terrain");

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "New_Terrain")
            {
                // create new gameobject for storing terrain blocks
                GameObject terrianStorage = new GameObject();
                terrianStorage.name = "terrainStore";

                createFirstTerrainCube(terrianStorage);
            }
        };
    }


    private void createFirstTerrainCube(GameObject terrianStorage)
    {
        //createFirstCube();
        // create new cube primitive

        GameObject firstCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstCube.transform.parent = terrianStorage.transform;

        Mesh cubeMesh = firstCube.GetComponent<MeshFilter>().mesh;
        // get the altas material and set the UV map of cube
        bool isTerrain = true;
        Material material = base.createCube(isTerrain, cubeMesh);

        
        firstCube.GetComponent<Renderer>().material = material;





        // Create a new Terrain GameObject
        /*GameObject terrainGO = new GameObject("GeneratedTerrain");
        Terrain terrain = terrainGO.AddComponent<Terrain>();
        TerrainData terrainData = new TerrainData();

        // Set terrain size
        terrainData.size = new Vector3(width, 50, depth); // 50 is the height scale

        // Assign terrain data
        terrain.terrainData = terrainData;

        // Add a Terrain Collider
        TerrainCollider terrainCollider = terrainGO.AddComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;*/
    }

    private void createFirstTerrainCube()
    {
        // create new cube
        
        // find texture atlas and 
    }
}
