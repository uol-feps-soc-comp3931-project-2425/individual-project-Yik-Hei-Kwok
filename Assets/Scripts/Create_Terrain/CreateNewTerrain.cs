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
                instansiateTerrainCubes(x,z, terrianStorage);

            }
        };
    }


    private void createFirstTerrainCube(GameObject terrianStorage)
    {
        // create new cube primitive
        GameObject firstCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstCube.transform.parent = terrianStorage.transform;

        Mesh cubeMesh = firstCube.GetComponent<MeshFilter>().mesh;
        // get the altas material and set the UV map of cube
        bool isTerrain = true;
        Material material = base.createCube(isTerrain, cubeMesh);

        
        firstCube.GetComponent<Renderer>().material = material;

    }

    private void instansiateTerrainCubes(int width, int height, GameObject terrianStorage)
    {
        // instansiate every block, add them into the terrianStorage object, and give them collider
        GameObject cubePrefab = terrianStorage.transform.GetChild(0).gameObject;
        for (int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                GameObject newCube = Instantiate(cubePrefab, new Vector3(j, 0 , i), Quaternion.identity);
                newCube.transform.parent = terrianStorage.transform;
                newCube.AddComponent<BoxCollider>();
            }
        }
    }




    private void createFirstTerrainCube()
    {
        // create new cube
        
        // find texture atlas and 
    }
}
